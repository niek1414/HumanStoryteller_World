using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Helper.IntentHelper;
using HumanStoryteller.Incidents.Jobs;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_IntentGiver : HumanIncidentWorker {
    public const String Name = "IntentGiver";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_IntentGiver)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_IntentGiver allParams = Tell.AssertNotNull((HumanIncidentParams_IntentGiver) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        var pawns = new List<Pawn>();
        foreach (var pawn in allParams.Pawns.Filter(map)) {
            if (!pawn.DestroyedOrNull() && !pawn.Dead && pawn.Spawned) {
                pawns.Add(pawn);
            }
        }

        if (pawns.Count <= 0) {
            Tell.Warn("No pawns found to give intent to");
            return ir;
        }

        var lordJob = ResolveLordJob(pawns, allParams, map);
        if (lordJob == null) {
            var job = ResolveJob(pawns, allParams, map);
            if (job != null) {
                job.playerForced = true;
                var expiryInterval = Mathf.RoundToInt(allParams.FirstNumberParam.GetValue());
                job.expiryInterval = expiryInterval == -1 ? 999999 : expiryInterval;
                foreach (var pawn in pawns) {
                        pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.ForcedToJoinOtherLord);
                    if (!allParams.Queue) {
                    }

                    if (job.targetA.Cell.Equals(new IntVec3(-1, -2, -3))) {
                        job.targetA = pawn.Position;
                    }

                    if (allParams.Queue) {
                        pawn.jobs.jobQueue.EnqueueLast(job);
                    } else {
                        pawn.jobs.ClearQueuedJobs();
                        pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                    }
                }
            }
        } else {
            foreach (var t in pawns) {
                t.GetLord()?.Notify_PawnLost(t, PawnLostCondition.ForcedToJoinOtherLord);
            }

            if (allParams.IntentType.Equals("Travel")) {
                IncidentResult_Traveled traveledIR = new IncidentResult_Traveled();
                LordUtil.MakeNewLord(pawns[0].Faction, lordJob, map, traveledIR, pawns);
                ir = traveledIR;
            } else {
                LordMaker.MakeNewLord(pawns[0].Faction, lordJob, map, pawns);
            }

            if (allParams.IntentType.Equals("Travel") || allParams.IntentType.Equals("TravelAndExit") ||
                allParams.IntentType.Equals("DefendPoint")) {
                foreach (var t in pawns) {
                    t.mindState.duty.locomotion = GetUrgencyFromString(allParams.SecondStringParam);
                }
            }

            foreach (var t in pawns) {
                t.jobs.ClearQueuedJobs();
                t.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        SendLetter(allParams);

        return ir;
    }

    private LordJob ResolveLordJob(List<Pawn> list, HumanIncidentParams_IntentGiver allParams, Map map) {
        switch (allParams.IntentType) {
            case "Steal":
                return new LordJob_Steal();
            case "DefendAttackedTraderCaravan": //Requires a trader in the group
                return new LordJob_DefendAttackedTraderCaravan(list[0].Position);
            case "ManTurrets":
                return new LordJob_ManTurrets();
            case "Joinable_MarriageCeremony":
                Pawn p1 = PawnUtil.GetPawnByName(allParams.FirstStringParam);
                Pawn p2 = PawnUtil.GetPawnByName(allParams.SecondStringParam);
                if (p1.DestroyedOrNull() || p2.DestroyedOrNull() || p1.Dead || p2.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam, allParams.SecondStringParam);
                    return null;
                }

                if (!RCellFinder.TryFindMarriageSite(p1, p2, out IntVec3 result)) {
                    Tell.Warn("Didn't find marriage site for pawns: ", allParams.FirstStringParam, allParams.SecondStringParam);
                    return null;
                }

                return new LordJob_Joinable_MarriageCeremony(p1, p2, result);
            case "PrisonBreak":
                if (!RCellFinder.TryFindRandomExitSpot(list[0], out IntVec3 spot, TraverseMode.PassDoors) ||
                    !TryFindGroupUpLoc(list, spot, out IntVec3 groupUpLoc)) {
                    Tell.Warn("Didn't find escape route for prison break.");
                    return null;
                }

                return new LordJob_PrisonBreak(groupUpLoc, spot, Rand.Value < 0.5f ? list[0].thingIDNumber : -1);
            case "DefendAndExpandHive":
                var pawnParams = new SpawnedPawnParams();
                pawnParams.aggressive = true;
                return new LordJob_DefendAndExpandHive(pawnParams);
            case "StageThenAttack":
                IntVec3 entrySpot1 = list[0].PositionHeld;
                IntVec3 stageLoc1 = RCellFinder.FindSiegePositionFrom(entrySpot1, map);
                return new LordJob_StageThenAttack(list[0].Faction, stageLoc1, Rand.Int);
            case "Siege":
                IntVec3 entrySpot2 = list[0].PositionHeld;
                IntVec3 stageLoc2 = RCellFinder.FindSiegePositionFrom(entrySpot2, map);
                float num = StorytellerUtility.DefaultThreatPointsNow(map) *
                            (allParams.FirstNumberParam.GetValue() != -1 ? allParams.FirstNumberParam.GetValue() : 1) * Rand.Range(0.2f, 0.3f);
                if (num < 60f) {
                    num = 60f;
                }

                return new LordJob_Siege(list[0].Faction, stageLoc2, num);
            case "Joinable_Party":
                if (!RCellFinder.TryFindGatheringSpot(list[0], GatheringDefOf.Party, true, out IntVec3 result1)) {
                    Tell.Warn("Didn't find party spot.");
                    return null;
                }

                return new LordJob_Joinable_Party(result1, list[0], GatheringDefOf.Party);
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
                return new LordJob_SleepThenAssaultColony(list[0].Faction);
            case "TravelAndExit":
                return new LordJob_TravelAndExit(allParams.Location.GetSingleCell(map));
            case "Travel":
                return new LordJob_TravelExact(allParams.Location.GetSingleCell(map));
            case "ExitMapBest":
                return new LordJob_ExitMapBest(GetUrgencyFromString(allParams.FirstStringParam));
            case "DefendPoint":
                return new LordJob_DefendPoint(allParams.Location.GetSingleCell(map));
            default:
                Tell.Warn("Didn't resolve intent type (lord).");
                return null;
        }
    }

    private Job ResolveJob(List<Pawn> list, HumanIncidentParams_IntentGiver allParams, Map map) {
        switch (allParams.IntentType) {
            case "Hunt":
                var p1 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p1.DestroyedOrNull() || p1.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Hunt, p1);
            case "Wait":
                return JobMaker.MakeJob(JobDefOf.Wait);
            case "AttackMelee":
                var p2 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p2 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.AttackMelee, p2);
            case "AttackStatic":
                var p3 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p3 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.AttackStatic, p3);
            case "Follow":
                var p4 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p4 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Follow, p4);
            case "FollowClose":
                var p5 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p5 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                var job = JobMaker.MakeJob(JobDefOf.FollowClose, p5);
                job.followRadius = 3f;
                return job;
            case "Strip":
                var p6 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p6 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Strip, p6);
            case "TradeWithPawn":
                var p7 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p7.DestroyedOrNull() || p7.Dead || p7.Downed) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.TradeWithPawn, p7);
            case "SocialFight":
                var p8 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p8.DestroyedOrNull() || p8.Dead || p8.Downed) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                if (!InteractionUtility.TryGetRandomVerbForSocialFight(p8, out Verb verb)) {
                    return null;
                }

                var job2 = JobMaker.MakeJob(JobDefOf.SocialFight, p8);
                job2.verbToUse = verb;
                return job2;
            case "Insult":
                var p9 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p9.DestroyedOrNull() || p9.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Insult, p9);
            case "Ignite":
                var p10 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p10 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Ignite, p10);
            case "LayDown":
                return JobMaker.MakeJob(JobDefOf.LayDown, new LocalTargetInfo(new IntVec3(-1, -2, -3)));
            case "Rescue":
                var p12 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p12.DestroyedOrNull() || p12.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                Building_Bed buildingBed = RestUtility.FindBedFor(p12, list[0], false, false, GuestStatus.Guest);
                if (buildingBed == null || !p12.CanReserve(buildingBed)) {
                    Tell.Warn("Didn't find bed", allParams.FirstStringParam);
                    return null;
                }

                Job job3 = JobMaker.MakeJob(JobDefOf.Rescue, p12, buildingBed);
                job3.count = 1;
                return job3;
            case "Capture":
                var p13 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p13.DestroyedOrNull() || p13.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                Building_Bed buildingBed2 = RestUtility.FindBedFor(p13, list[0], true, false, GuestStatus.Guest);
                if (buildingBed2 == null || !p13.CanReserve(buildingBed2)) {
                    Tell.Warn("Didn't find bed", allParams.FirstStringParam);
                    return null;
                }

                Job job4 = JobMaker.MakeJob(JobDefOf.Capture, p13, buildingBed2);
                job4.count = 1;
                return job4;
            case "ReleasePrisoner":
                var p14 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p14 == null) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                if (!RCellFinder.TryFindPrisonerReleaseCell(p14, list[0], out IntVec3 result)) {
                    return null;
                }

                Job job5 = JobMaker.MakeJob(JobDefOf.ReleasePrisoner, p14, result);
                job5.count = 1;
                return job5;
            case "Kidnap":
                var p15 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p15.DestroyedOrNull() || p15.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                if (!RCellFinder.TryFindBestExitSpot(p15, out IntVec3 spot)) {
                    return null;
                }

                Job job6 = JobMaker.MakeJob(JobDefOf.Kidnap, p15, spot);
                job6.count = 1;
                return job6;
            case "PrisonerExecution":
                var p16 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p16.DestroyedOrNull() || p16.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.PrisonerExecution, p16);
            case "Slaughter":
                var p17 = PawnUtil.GetPawnByName(allParams.FirstStringParam);

                if (p17.DestroyedOrNull() || p17.Dead) {
                    Tell.Warn("Didn't find pawn while resolving job", allParams.FirstStringParam);
                    return null;
                }

                return JobMaker.MakeJob(JobDefOf.Slaughter, p17);
            case "Vomit":
                return JobMaker.MakeJob(JobDefOf.Vomit);
            case "UnloadYourInventory":
                return JobMaker.MakeJob(JobDefOf.UnloadYourInventory);
            default:
                Tell.Warn("Didn't resolve intent type (job).");
                return null;
        }
    }

    private static LocomotionUrgency GetUrgencyFromString(string str) {
        switch (str) {
            case "None":
                return LocomotionUrgency.None;
            case "Amble":
                return LocomotionUrgency.Amble;
            case "Walk":
                return LocomotionUrgency.Walk;
            case "Jog":
                return LocomotionUrgency.Jog;
            case "Sprint":
                return LocomotionUrgency.Sprint;
            default:
                return LocomotionUrgency.Walk;
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
                if (room != null && !room.IsPrisonCell && (room.TouchesMapEdge || room.IsHuge || room.Cells.Count(x => x.Standable(map)) >= 5)) {
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

public class HumanIncidentParams_IntentGiver : HumanIncidentParams {
    public PawnGroupSelector Pawns = new PawnGroupSelector();
    public string IntentType = "";
    public string FirstStringParam = "";
    public string SecondStringParam = "";
    public Location Location = new Location();
    public Number FirstNumberParam = new Number();
    public bool Queue;

    public HumanIncidentParams_IntentGiver() {
    }

    public HumanIncidentParams_IntentGiver(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return
            $"{base.ToString()}, Pawns: [{Pawns}], IntentType: [{IntentType}], FirstStringParam: [{FirstStringParam}], SecondStringParam: [{SecondStringParam}], FirstNumberParam: [{FirstNumberParam}], FirstBoolParam: [{Queue}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Pawns, "names");
        Scribe_Values.Look(ref IntentType, "type");
        Scribe_Values.Look(ref FirstStringParam, "firstStringParam");
        Scribe_Values.Look(ref SecondStringParam, "secondStringParam");
        Scribe_Deep.Look(ref FirstNumberParam, "firstNumberParam");
        Scribe_Deep.Look(ref Location, "location");
        Scribe_Values.Look(ref Queue, "queue");
    }
}