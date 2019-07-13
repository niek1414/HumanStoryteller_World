using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CreatePawn : HumanIncidentWorker {
        public const String Name = "CreatePawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_CreatePawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_CreatePawn
                allParams = Tell.AssertNotNull((HumanIncidentParams_CreatePawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore,
                out IntVec3 cell)) {
                Tell.Err("No path found from the edge to the colony.");
                return ir;
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
                1F, true, true, true, false, false, false, false, null, null, null,
                biologicalAge == -1 ? new float?() : biologicalAge,
                chronologicalAge == -1 ? new float?() : chronologicalAge,
                allParams.Gender == "" || PawnUtil.GetGender(allParams.Gender) == Gender.None ? new Gender?() : PawnUtil.GetGender(allParams.Gender)
            ));

            PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);
            
            if (pawn.Faction == Faction.OfPlayer)
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);

            if (allParams.OutName != "") {
                PawnUtil.SavePawnByName(allParams.OutName, pawn);
            }

            if (allParams.Weapon != "") {
                if (pawn.equipment.Primary != null)
                    pawn.equipment.Remove(pawn.equipment.Primary);
                if (allParams.Weapon != "None") {
                    var qc = allParams.ItemQuality != "" ? ItemUtil.GetCategory(allParams.ItemQuality) : QualityCategory.Normal;
                    ThingDef thingDef = (from def in DefDatabase<ThingDef>.AllDefs
                        where def.IsWeapon && def.defName.Equals(allParams.Weapon)
                        select def).RandomElementWithFallback();
                    ThingDef stuff = GenStuff.RandomStuffFor(thingDef);
                    try {
                        if (allParams.Stuff != "") {
                            stuff = (from d in DefDatabase<ThingDef>.AllDefs
                                where d.IsStuff && d.defName.Equals(allParams.Stuff)
                                select d).First();
                        }
                    } catch (InvalidOperationException) {
                    }

                    var weapon = (ThingWithComps) ThingMaker.MakeThing(thingDef, stuff);
                    ItemUtil.TrySetQuality(weapon, qc);
                    pawn.equipment.AddEquipment(weapon);
                }
            }

            GenSpawn.Spawn(pawn, cell, map);
            if (pawn.Faction == Faction.OfPlayer)
                return ir;
            IncidentParms fakeParms = new IncidentParms();
            fakeParms.faction = faction;
            fakeParms.target = map;
            fakeParms.pawnGroups = new Dictionary<Pawn, int> {{pawn, 0}};
            fakeParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            RaidStrategyDefOf.ImmediateAttack.Worker.MakeLords(fakeParms, new List<Pawn> {pawn});

            SendLetter(parms);
            return ir;
        }
    }

    public class HumanIncidentParams_CreatePawn : HumanIncidentParms {
        public Number BiologicalAge = new Number();
        public Number ChronologicalAge = new Number();
        public Number ApparelMoney = new Number();
        public Number GearHealthMin;
        public Number GearHealthMax = new Number();
        public string PawnKind = "";
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";
        public string OutName = "";
        public string Faction = "";
        public bool NewBorn;
        public bool MustBeCapableOfViolence;
        public string Gender = "";
        public string Weapon = "";
        public string ItemQuality = "";
        public string Stuff = "";

        public HumanIncidentParams_CreatePawn() {
        }

        public HumanIncidentParams_CreatePawn(string target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, BiologicalAge: {BiologicalAge}, ChronologicalAge: {ChronologicalAge}, ApparelMoney: {ApparelMoney}, GearHealthMin: {GearHealthMin}, GearHealthMax: {GearHealthMax}, PawnKind: {PawnKind}, FirstName: {FirstName}, NickName: {NickName}, LastName: {LastName}, OutName: {OutName}, Faction: {Faction}, NewBorn: {NewBorn}, MustBeCapableOfViolence: {MustBeCapableOfViolence}, Gender: {Gender}, Weapon: {Weapon}, ItemQuality: {ItemQuality}, Stuff: {Stuff}";
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
            Scribe_Values.Look(ref MustBeCapableOfViolence, "mustBeCapableOfViolence");
            Scribe_Values.Look(ref Gender, "gender");
            Scribe_Values.Look(ref Weapon, "weapon");
            Scribe_Values.Look(ref ItemQuality, "itemQuality");
            Scribe_Values.Look(ref Stuff, "stuff");
        }
    }
}