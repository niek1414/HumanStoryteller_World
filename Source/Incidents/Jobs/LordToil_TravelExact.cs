using HumanStoryteller.Util;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents.Jobs {
    public class LordToil_TravelExact : LordToil {
        public Danger maxDanger;

        public LordToil_TravelExact() {
        }

        public LordToil_TravelExact(IntVec3 dest) {
            data = new LordToilData_Travel();
            Data.dest = dest;
        }

        public override IntVec3 FlagLoc => Data.dest;

        private LordToilData_Travel Data => (LordToilData_Travel) data;

        public override bool AllowSatisfyLongNeeds => false;

        protected virtual float AllArrivedCheckRadiusSingle => 3f;

        protected virtual float AllArrivedCheckRadiusGroup => 10f;

        public override void UpdateAllDuties() {
            LordToilData_Travel data = Data;
            for (int index = 0; index < lord.ownedPawns.Count; ++index)
                if (index == 0) {
                    lord.ownedPawns[index].mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("TravelExact"), data.dest) {
                        maxDanger = maxDanger
                    };
                } else {
                    lord.ownedPawns[index].mindState.duty = new PawnDuty(DutyDefOf.TravelOrLeave, data.dest) {
                        maxDanger = maxDanger
                    };
                }
        }

        public override void LordToilTick() {
            if (Find.TickManager.TicksGame % 30 != 0)
                return;
            LordToilData_Travel data = Data;
            bool foundNotArrived = false;
            bool foundLiving = false;
            for (int index = 0; index < lord.ownedPawns.Count; ++index) {
                Pawn ownedPawn = lord.ownedPawns[index];
                if (ownedPawn == null || ownedPawn.Dead || ownedPawn.Destroyed || !ownedPawn.Spawned) {
                    continue;
                }
                
                foundLiving = true;
                if (!ownedPawn.Position.InHorDistOf(data.dest,
                        lord.ownedPawns.Count > 1 ? AllArrivedCheckRadiusGroup : AllArrivedCheckRadiusSingle) || !ownedPawn.CanReach(data.dest,
                        PathEndMode.ClosestTouch, Danger.Deadly)) {
                    foundNotArrived = true;
                    break;
                }
            }

            if (foundNotArrived || !foundLiving)
                return;
            lord.ReceiveMemo("TravelArrived");
        }

        public bool HasDestination() {
            return Data.dest.IsValid;
        }

        public void SetDestination(IntVec3 dest) {
            Data.dest = dest;
        }
    }
}