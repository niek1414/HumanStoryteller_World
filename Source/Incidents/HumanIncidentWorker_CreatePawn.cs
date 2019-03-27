using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CreatePawn : HumanIncidentWorker {
        public const String Name = "CreatePawn";

        public override IncidentResult Execute(HumanIncidentParms parms) {
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
            if (allParams.ApparelMoney != -1) {
                pawnKind.apparelMoney = new FloatRange(allParams.ApparelMoney * pawnKind.apparelMoney.min,
                    allParams.ApparelMoney * pawnKind.apparelMoney.max);
            }

            if (allParams.GearHealthMin != -1) {
                pawnKind.gearHealthRange.min = allParams.GearHealthMin;
            }

            if (allParams.GearHealthMax != -1) {
                pawnKind.gearHealthRange.max = allParams.GearHealthMax;
            }

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
                allParams.BiologicalAge == -1 ? new float?() : allParams.BiologicalAge,
                allParams.ChronologicalAge == -1 ? new float?() : allParams.ChronologicalAge,
                allParams.Gender == "" || PawnUtil.GetGender(allParams.Gender) == Gender.None ? new Gender?() : PawnUtil.GetGender(allParams.Gender),
                null, allParams.Name != "" ? allParams.Name : null
            ));

            if (pawn.Faction == Faction.OfPlayer)
                Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);

            if (allParams.Name != "") {
                switch (pawn.Name) {
                    case NameTriple prevNameTriple:
                        pawn.Name = new NameTriple(allParams.Name, prevNameTriple.Nick, prevNameTriple.Last);
                        break;
                    case NameSingle _:
                        pawn.Name = new NameSingle(allParams.Name);
                        break;
                    default:
                        pawn.Name = new NameTriple(allParams.Name, allParams.Name, "");
                        break;
                }
                PawnUtil.SavePawnByName(allParams.Name, pawn);
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
            if (parms.Letter?.Type != null) {
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_CreatePawn : HumanIncidentParms {
        public string PawnKind;
        public string Name;
        public string Faction;
        public bool NewBorn;
        public bool MustBeCapableOfViolence;
        public float BiologicalAge;
        public float ChronologicalAge;
        public string Gender;
        public string Weapon;
        public string ItemQuality;
        public string Stuff;
        public float ApparelMoney;
        public float GearHealthMin;
        public float GearHealthMax;

        public HumanIncidentParams_CreatePawn() {
        }

        public HumanIncidentParams_CreatePawn(String target, HumanLetter letter, string pawnKind = "", string name = "", string faction = "",
            bool newBorn = false, bool mustBeCapableOfViolence = false, float biologicalAge = -1, float chronologicalAge = -1, string gender = "",
            string weapon = "", string itemQuality = "", string stuff = "", float apparelMoney = -1, float gearHealthMin = -1,
            float gearHealthMax = -1) :
            base(target, letter) {
            PawnKind = pawnKind;
            Name = name;
            Faction = faction;
            NewBorn = newBorn;
            MustBeCapableOfViolence = mustBeCapableOfViolence;
            BiologicalAge = biologicalAge;
            ChronologicalAge = chronologicalAge;
            Gender = gender;
            Weapon = weapon;
            ItemQuality = itemQuality;
            Stuff = stuff;
            ApparelMoney = apparelMoney;
            GearHealthMin = gearHealthMin;
            GearHealthMax = gearHealthMax;
        }

        public override string ToString() {
            return
                $"{base.ToString()}, PawnKind: {PawnKind}, Name: {Name}, Faction: {Faction}, NewBorn: {NewBorn}, MustBeCapableOfViolence: {MustBeCapableOfViolence}, BiologicalAge: {BiologicalAge}, ChronologicalAge: {ChronologicalAge}, Gender: {Gender}, Weapon: {Weapon}, ItemQuality: {ItemQuality}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref PawnKind, "pawnKind");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref NewBorn, "newBorn");
            Scribe_Values.Look(ref MustBeCapableOfViolence, "mustBeCapableOfViolence");
            Scribe_Values.Look(ref BiologicalAge, "biologicalAge");
            Scribe_Values.Look(ref ChronologicalAge, "chronologicalAge");
            Scribe_Values.Look(ref Gender, "gender");
            Scribe_Values.Look(ref Weapon, "weapon");
            Scribe_Values.Look(ref ItemQuality, "itemQuality");
            Scribe_Values.Look(ref Stuff, "stuff");
            Scribe_Values.Look(ref ApparelMoney, "apparelMoney");
            Scribe_Values.Look(ref GearHealthMin, "gearHealthMin");
            Scribe_Values.Look(ref GearHealthMax, "gearHealthMax");
        }
    }
}