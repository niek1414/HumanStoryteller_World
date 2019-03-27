using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Alphabeavers : HumanIncidentWorker {
        public const String Name = "Alphabeavers";

        private static readonly FloatRange CountPerColonistRange = new FloatRange(1f, 1.5f);

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Alphabeavers)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Alphabeavers allParams = Tell.AssertNotNull((HumanIncidentParams_Alphabeavers) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) {
                result = CellFinder.RandomEdgeCell(map);
            }

            int num;
            if (allParams.Amount != -1) {
                num = Mathf.RoundToInt(allParams.Amount);
            } else {
                int freeColonistsCount = map.mapPawns.FreeColonistsCount;
                float randomInRange = CountPerColonistRange.RandomInRange;
                float f = freeColonistsCount * randomInRange;
                num = Mathf.Clamp(GenMath.RoundRandom(f), 1, 10);
            }

            for (int i = 0; i < num; i++) {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
                Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver);
                Pawn pawn = (Pawn) GenSpawn.Spawn(newThing, loc, map);
                pawn.needs.food.CurLevelPercentage = 1f;
            }


            SendLetter(allParams, "LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterDefOf.ThreatSmall,
                new TargetInfo(result, map));

            return ir;
        }
    }

    public class HumanIncidentParams_Alphabeavers : HumanIncidentParms {
        public Number Amount;

        public HumanIncidentParams_Alphabeavers() {
        }

        public HumanIncidentParams_Alphabeavers(String target, HumanLetter letter, Number amount) : base(target, letter) {
            Amount = amount;
        }

        public HumanIncidentParams_Alphabeavers(String target, HumanLetter letter) : this(target, letter, new Number())  {
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}";
        }
        
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Amount, "amount");
        }
    }
}