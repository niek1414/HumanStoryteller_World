using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_HerdMigration : HumanIncidentWorker {
        public const String Name = "HerdMigration";

        private static readonly IntRange AnimalsCount = new IntRange(3, 5);

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_HerdMigration)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_HerdMigration allParams =
                Tell.AssertNotNull((HumanIncidentParams_HerdMigration) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            PawnKindDef kindDef;
            if (allParams.AnimalKind != "") {
                kindDef = PawnKindDef.Named(allParams.AnimalKind);
            } else {
                if (!TryFindAnimalKind(map.Tile, out PawnKindDef animalKind)) {
                    return ir;
                }

                kindDef = animalKind;
            }

            if (!TryFindStartAndEndCells(map, out IntVec3 start, out IntVec3 end)) {
                return ir;
            }

            Rot4 rot = Rot4.FromAngleFlat((map.Center - start).AngleFlat);

            int randomInRange;
            var amount = allParams.Amount.GetValue();
            if (amount != -1) {
                randomInRange = Mathf.RoundToInt(amount);
            } else {
                randomInRange = AnimalsCount.RandomInRange;
                randomInRange = Mathf.Max(randomInRange, Mathf.CeilToInt(4f / kindDef.RaceProps.baseBodySize));
            }

            List<Pawn> list = GenerateAnimals(kindDef, randomInRange, map.Tile);
            for (int i = 0; i < list.Count; i++) {
                Pawn newThing = list[i];
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(start, map, 10);
                GenSpawn.Spawn(newThing, loc, map, rot);
            }

            LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(end, LocomotionUrgency.Walk), map, list);
            var def = IncidentDef.Named(Name);
            string text = string.Format(def.letterText, kindDef.GetLabelPlural()).CapitalizeFirst();
            string label = string.Format(def.letterLabel, kindDef.GetLabelPlural().CapitalizeFirst());
            SendLetter(allParams, label, text, def.letterDef, list.Count < 1 ? null : list[0]);
            return ir;
        }

        private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind) {
            return (from k in DefDatabase<PawnKindDef>.AllDefs
                where k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)
                select k).TryRandomElementByWeight(x => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
        }

        private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end) {
            if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal)) {
                end = IntVec3.Invalid;
                return false;
            }

            end = IntVec3.Invalid;
            for (int i = 0; i < 8; i++) {
                IntVec3 startLocal = start;
                if (!CellFinder.TryFindRandomEdgeCellWith(
                    x => map.reachability.CanReach(startLocal, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly), map,
                    CellFinder.EdgeRoadChance_Ignore, out IntVec3 result)) {
                    break;
                }

                if (!end.IsValid || result.DistanceToSquared(start) > end.DistanceToSquared(start)) {
                    end = result;
                }
            }

            return end.IsValid;
        }

        private List<Pawn> GenerateAnimals(PawnKindDef animalKind, int animalsCount, int tile) {
            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < animalsCount; i++) {
                PawnGenerationRequest request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile, false, false,
                    false, false, true, false, 1f, false, true, true, false, false, false, false, null, null, null, null, null, null, null);
                Pawn item = PawnGenerator.GeneratePawn(request);
                list.Add(item);
            }

            return list;
        }
    }

    public class HumanIncidentParams_HerdMigration : HumanIncidentParms {
        public Number Amount = new Number();
        public string AnimalKind = "";

        public HumanIncidentParams_HerdMigration() {
        }

        public HumanIncidentParams_HerdMigration(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: [{Amount}], Kind: [{AnimalKind}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref AnimalKind, "animalKind");
        }
    }
}