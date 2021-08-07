using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CreatePawn : HumanIncidentWorker {
        public const String Name = "CreatePawn";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_CreatePawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_CreatePawn
                allParams = Tell.AssertNotNull((HumanIncidentParams_CreatePawn) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            IntVec3 cell;
            
            var optCell = allParams.Location.GetSingleCell(map, false);
            if (optCell.IsValid) {
                cell = optCell;
            } else {
                if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore,
                    out cell)) {
                    Tell.Err("No path found from the edge to the colony.");
                    return ir;
                }
            }
           
            Faction faction;
            try {
                faction = Find.FactionManager.AllFactions.First(f => f.def.defName == allParams.Faction);
            } catch (InvalidOperationException) {
                faction = Faction.OfPlayer;
            }

            PawnKindDef pawnKind = PawnKindDef.Named(allParams.PawnKind) ?? PawnKindDefOf.Colonist;
            var money = allParams.ApparelMoney.GetValue();
            if (money != -1) {
                pawnKind.apparelMoney = new FloatRange(money * pawnKind.apparelMoney.min,
                    money * pawnKind.apparelMoney.max);
            }

            var gearHealthMin = allParams.GearHealthMin.GetValue();
            if (gearHealthMin != -1) {
                pawnKind.gearHealthRange.min = gearHealthMin;
            }

            var gearHealthMax = allParams.GearHealthMax.GetValue();
            if (gearHealthMax != -1) {
                pawnKind.gearHealthRange.max = gearHealthMax;
            }

            var biologicalAge = allParams.BiologicalAge.GetValue();
            var chronologicalAge = allParams.ChronologicalAge.GetValue();
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                pawnKind,
                faction,
                PawnGenerationContext.NonPlayer,
                map.Tile,
                false,
                allParams.NewBorn,
                false, false, true,
                allParams.MustBeCapableOfViolence,
                1F, false, true, true, false, false, false, false, false, 0F, 0F, null, 1F, null, null, null, null, null,
                biologicalAge == -1 ? new float?() : biologicalAge,
                chronologicalAge == -1 ? new float?() : chronologicalAge,
                allParams.Gender == "" || PawnUtil.GetGender(allParams.Gender) == Gender.None ? new Gender?() : PawnUtil.GetGender(allParams.Gender)
            ));
            var graphicsChanged = false;

            PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);

            if (pawn.Faction == Faction.OfPlayer)
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);

            if (allParams.OutName != "") {
                PawnUtil.SavePawnByName(allParams.OutName, pawn);
            }

            if (allParams.HairType != "") {
                var hairDef = DefDatabase<HairDef>.GetNamed(allParams.HairType, false);
                if (hairDef != null) {
                    pawn.story.hairDef = hairDef;
                    graphicsChanged = true;
                } else {
                    Tell.Warn("Did not find hair def with name: " + allParams.HairType);
                }
            }

            if (allParams.HairColor != "") {
                var optColor = PawnUtil.HexToColor(allParams.HairColor);
                if (optColor.HasValue) {
                    Tell.Debug("Found color: " + optColor);
                    pawn.story.hairColor = optColor.Value;
                    graphicsChanged = true;
                } else {
                    Tell.Log("Tried to set hair color but could not do to the warning above.");
                }
            }

            if (allParams.BodyType != "") {
                var bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamed(allParams.BodyType, false);
                if (bodyTypeDef != null) {
                    pawn.story.bodyType = bodyTypeDef;
                    graphicsChanged = true;
                } else {
                    Tell.Warn("Did not find body type def with name: " + allParams.BodyType);
                }
            }

            var melanin = allParams.Melanin.GetValue();
            if (melanin != -1) {
                pawn.story.melanin = melanin;
                graphicsChanged = true;
            }

            if (allParams.Weapon.NotEmpty()) {
                if (pawn.equipment.Primary != null)
                    pawn.equipment.Remove(pawn.equipment.Primary);
                if (allParams.Weapon.Thing != "None") {
                    var weapon = allParams.Weapon.GetThing();
                    if (weapon != null) {
                        pawn.equipment.AddEquipment(weapon);
                    }
                }
            }
                
            if (allParams.RestrictedArea.isSet()) {
                if (pawn.playerSettings == null) {
                    pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                }
                var areaAllowed = new Area_Allowed(map.areaManager, "RestrictedArea" + pawn.LabelShort + Rand.Int);
                pawn.playerSettings.AreaRestriction = areaAllowed;
                allParams.RestrictedArea.GetZone(map).Cells.ForEach(zoneCell => {
                    var intVec3 = zoneCell.Pos;
                    if (intVec3.IsValid && intVec3.InBounds(map)) {
                        areaAllowed[intVec3] = true;
                    }
                });
            }
            
            if (allParams.NoSpawn) {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
            } else {
                GenSpawn.Spawn(pawn, cell, map);

                if (graphicsChanged) {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }

                if (pawn.Faction != Faction.OfPlayer) {
                    switch (pawn.Faction?.PlayerRelationKind ?? FactionRelationKind.Neutral) {
                        case FactionRelationKind.Hostile:
                            IncidentParms fakeParmsAttack = new IncidentParms();
                            fakeParmsAttack.faction = faction;
                            fakeParmsAttack.target = map;
                            fakeParmsAttack.pawnGroups = new Dictionary<Pawn, int> {{pawn, 0}};
                            fakeParmsAttack.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                            RaidStrategyDefOf.ImmediateAttack.Worker.MakeLords(fakeParmsAttack, new List<Pawn> {pawn});
                            break;
                        case FactionRelationKind.Neutral:
                            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out IntVec3 result3);
                            LordMaker.MakeNewLord(pawn.Faction, new LordJob_Travel(result3), map, new[] {pawn});
                            break;
                        case FactionRelationKind.Ally:
                            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out IntVec3 result4);
                            LordMaker.MakeNewLord(pawn.Faction, new LordJob_DefendBase(pawn.Faction, result4), map, new[] {pawn});
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }


            SendLetter(@params);
            return ir;
        }
    }

    public class HumanIncidentParams_CreatePawn : HumanIncidentParams {
        public Number BiologicalAge = new Number();
        public Number ChronologicalAge = new Number();
        public Number ApparelMoney = new Number();
        public Number GearHealthMin = new Number();
        public Number GearHealthMax = new Number();
        public string PawnKind = "";
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";
        public string OutName = "";
        public string Faction = "";
        public bool NewBorn;
        public bool NoSpawn;
        public bool MustBeCapableOfViolence;
        public string Gender = "";
        public Item Weapon = new Item();
        public string BodyType = "";
        public Number Melanin = new Number();
        public string HairType = "";
        public string HairColor = "";
        public Location Location = new Location();
        public Location RestrictedArea = new Location();

        public HumanIncidentParams_CreatePawn() {
        }

        public HumanIncidentParams_CreatePawn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return
                $"{base.ToString()}, BiologicalAge: [{BiologicalAge}], ChronologicalAge: [{ChronologicalAge}], ApparelMoney: [{ApparelMoney}], GearHealthMin: [{GearHealthMin}], GearHealthMax: [{GearHealthMax}], PawnKind: [{PawnKind}], FirstName: [{FirstName}], NickName: [{NickName}], LastName: [{LastName}], OutName: [{OutName}], Faction: [{Faction}], NewBorn: [{NewBorn}], NoSpawn: [{NoSpawn}], MustBeCapableOfViolence: [{MustBeCapableOfViolence}], Gender: [{Gender}], Weapon: [{Weapon}], BodyType: [{BodyType}], Melanin: [{Melanin}], HairType: [{HairType}], HairColor: [{HairColor}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref BiologicalAge, "biologicalAge");
            Scribe_Deep.Look(ref ChronologicalAge, "chronologicalAge");
            Scribe_Deep.Look(ref ApparelMoney, "apparelMoney");
            Scribe_Deep.Look(ref GearHealthMin, "gearHealthMin");
            Scribe_Deep.Look(ref GearHealthMax, "gearHealthMax");
            Scribe_Values.Look(ref PawnKind, "pawnKind");
            Scribe_Values.Look(ref FirstName, "firstName");
            Scribe_Values.Look(ref NickName, "nickName");
            Scribe_Values.Look(ref LastName, "lastName");
            Scribe_Values.Look(ref OutName, "name");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref NewBorn, "newBorn");
            Scribe_Values.Look(ref NoSpawn, "noSpawn");
            Scribe_Values.Look(ref MustBeCapableOfViolence, "mustBeCapableOfViolence");
            Scribe_Values.Look(ref Gender, "gender");
            Scribe_Deep.Look(ref Weapon, "weapon");
            Scribe_Deep.Look(ref Location, "location");
            Scribe_Deep.Look(ref RestrictedArea, "restrictedArea");

            Scribe_Values.Look(ref BodyType, "bodyType");
            Scribe_Deep.Look(ref Melanin, "melanin");
            Scribe_Values.Look(ref HairType, "hairType");
            Scribe_Values.Look(ref HairColor, "hairColor");
        }
    }
}