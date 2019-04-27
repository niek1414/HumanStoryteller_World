using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_IntentGiver : HumanIncidentWorker {
        public const String Name = "IntentGiver";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_IntentGiver)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_IntentGiver allParams = Tell.AssertNotNull((HumanIncidentParams_IntentGiver) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            var pawns = new List<Pawn>();
            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn != null) {
                    pawns.Add(pawn);
                }
            }

            if (pawns.Count <= 0) {
                return ir;
            }

            LordMaker.MakeNewLord(pawns[0].Faction, ResolveLordJob(pawns, allParams, map), map, pawns);

            SendLetter(allParams);

            return ir;
        }

        private LordJob ResolveLordJob(List<Pawn> list, HumanIncidentParams_IntentGiver allParams, Map map) {
            switch (allParams.Type) {
                case "Steal":
                    return new LordJob_Steal();
                case "DefendAttackedTraderCaravan": //Requires a trader in the group
                    return new LordJob_DefendAttackedTraderCaravan(list[0].Position);
                case "ManTurrets":
                    return new LordJob_ManTurrets();
                case "Joinable_MarriageCeremony":
                    Pawn p1 = PawnUtil.GetPawnByName(allParams.FirstStringParam);
                    Pawn p2 = PawnUtil.GetPawnByName(allParams.SecondStringParam);
                    if (p1 == null || p2 == null) {
                        return null;
                    }

                    if (!RCellFinder.TryFindMarriageSite(p1, p2, out IntVec3 result)) {
                        return null;
                    }

                    return new LordJob_Joinable_MarriageCeremony(p1, p2, result);
                case "PrisonBreak":
                    if (!RCellFinder.TryFindRandomExitSpot(list[0], out IntVec3 spot, TraverseMode.PassDoors) ||
                        !TryFindGroupUpLoc(list, spot, out IntVec3 groupUpLoc)) {
                        return null;
                    }

                    return new LordJob_PrisonBreak(groupUpLoc, spot, Rand.Value < 0.5f ? list[0].thingIDNumber : -1);
                case "DefendAndExpandHive":
                    return new LordJob_DefendAndExpandHive(true);
                case "StageThenAttack":
                    IntVec3 entrySpot1 = list[0].PositionHeld;
                    IntVec3 stageLoc1 = RCellFinder.FindSiegePositionFrom(entrySpot1, map);
                    return new LordJob_StageThenAttack(list[0].Faction, stageLoc1, Rand.Int);
                case "Siege":
                    IntVec3 entrySpot2 = list[0].PositionHeld;
                    IntVec3 stageLoc2 = RCellFinder.FindSiegePositionFrom(entrySpot2, map);
                    float num = StorytellerUtility.DefaultThreatPointsNow(map) * (allParams.FirstNumberParam.GetValue() != -1 ? allParams.FirstNumberParam.GetValue() : 1) * Rand.Range(0.2f, 0.3f);
                    if (num < 60f)
                    {
                        num = 60f;
                    }
                    return new LordJob_Siege(list[0].Faction, stageLoc2, num);
                case "Joinable_Party":
                    if (!RCellFinder.TryFindPartySpot(list[0], out IntVec3 result1))
                    {
                        return null;
                    }
                    return new LordJob_Joinable_Party(result1, list[0]);
                case "Kidnap":
                    return new LordJob_Kidnap();
                case "AssaultColony":
                    return new LordJob_AssaultColony(list[0].Faction);
                case "VisitColony":
                    RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out IntVec3 result3);
                    return new LordJob_VisitColony(list[0].Faction, result3);
                case "AssistColony":
                    RCellFinder.TryFindRandomSpotJustOutsideColony(list[0].PositionHeld, map, out IntVec3 result4);
                    return new LordJob_AssistColony(list[0].Faction, result4);
                case "DefendBase":
                    return new LordJob_DefendBase(list[0].Faction, map.Center);
                case "TradeWithColony":
                    RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out IntVec3 result5);
                    return new LordJob_TradeWithColony(list[0].Faction, result5);
                case "SleepThenAssaultColony":
                    return new LordJob_SleepThenAssaultColony(list[0].Faction, true);
                case "TravelAndExit":
                    return new LordJob_TravelAndExit();
                case "Travel":
                    return new LordJob_Travel();
                case "ExitMapBest":
                    return new LordJob_ExitMapBest();
                case "ExitMapNear":
                    return new LordJob_ExitMapNear();
                case "DefendPoint":
                    return new LordJob_DefendPoint();
            }
        }

        private static bool TryFindGroupUpLoc(List<Pawn> escapingPrisoners, IntVec3 exitPoint, out IntVec3 groupUpLoc) {
            groupUpLoc = IntVec3.Invalid;
            Map map = escapingPrisoners[0].Map;
            using (PawnPath pawnPath = map.pathFinder.FindPath(escapingPrisoners[0].Position, exitPoint,
                TraverseParms.For(escapingPrisoners[0], Danger.Deadly, TraverseMode.PassDoors))) {
                if (!pawnPath.Found) {
                    Log.Warning("Prison break: could not find path for prisoner " + escapingPrisoners[0] + " to the exit point.");
                    return false;
                }

                for (int i = 0; i < pawnPath.NodesLeftCount; i++) {
                    IntVec3 intVec = pawnPath.Peek(pawnPath.NodesLeftCount - i - 1);
                    Room room = intVec.GetRoom(map);
                    if (room != null && !room.isPrisonCell && (room.TouchesMapEdge || room.IsHuge || room.Cells.Count(x => x.Standable(map)) >= 5)) {
                        groupUpLoc = CellFinder.RandomClosewalkCellNear(intVec, map, 3);
                    }
                }
            }

            if (!groupUpLoc.IsValid) {
                groupUpLoc = escapingPrisoners[0].Position;
            }

            return true;
        }
    }

    public class HumanIncidentParams_IntentGiver : HumanIncidentParms {
        public List<string> Names;
        public string Type;
        public string FirstStringParam;
        public string SecondStringParam;
        public Number FirstNumberParam;
        public Number SecondNumberParam;

        public HumanIncidentParams_IntentGiver() {
            Names = new List<string>();
            Type = "";
            FirstStringParam = "";
            SecondStringParam = "";
            FirstNumberParam = new Number();
            SecondNumberParam = new Number();
        }

        public HumanIncidentParams_IntentGiver(string target, HumanLetter letter) : base(target, letter) {
            Names = new List<string>();
            Type = "";
            FirstStringParam = "";
            SecondStringParam = "";
            FirstNumberParam = new Number();
            SecondNumberParam = new Number();
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref Type, "type");
            Scribe_Values.Look(ref FirstStringParam, "firstStringParam");
            Scribe_Values.Look(ref SecondStringParam, "secondStringParam");
            Scribe_Deep.Look(ref FirstNumberParam, "firstNumberParam");
            Scribe_Deep.Look(ref SecondNumberParam, "secondNumberParam");
        }
    }
}