using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_VisitorGroup : HumanIncidentWorker {
        public const String Name = "VisitorGroup";

        private static readonly SimpleCurve PointsCurve = new SimpleCurve {
            new CurvePoint(45f, 0f), new CurvePoint(50f, 1f), new CurvePoint(100f, 1f), new CurvePoint(200f, 0.25f), new CurvePoint(300f, 0.1f),
            new CurvePoint(500f, 0f)
        };

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_VisitorGroup)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_VisitorGroup allParams =
                Tell.AssertNotNull((HumanIncidentParams_VisitorGroup) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            if (!CandidateFactions(map).TryRandomElement(out var factionResult) &&
                !CandidateFactions(map, true).TryRandomElement(out factionResult)) {
                return ir;
            }

            IntVec3 spawnCenter = !RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 cellResult, map, CellFinder.EdgeRoadChance_Friendly) ? CellFinder.RandomEdgeCell(map) : cellResult;

            var fakeParams = new IncidentParms {
                faction = factionResult,
                spawnCenter = spawnCenter,
                points = TraderCaravanUtility.GenerateGuardPoints(),
                forced = true,
                target = map
            };
            List<Pawn> list = SpawnPawns(map, fakeParams);
            if (list.Count == 0)
            {
                return ir;
            }
            RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out IntVec3 result);
            LordJob_VisitColony lordJob = new LordJob_VisitColony(factionResult, result);
            LordMaker.MakeNewLord(factionResult, lordJob, map, list);

            Pawn pawn = list.Find(x => factionResult.leader == x);
            string letterLabel;
            string letterText;
            if (list.Count == 1) {
                string value = string.Empty;
                string value2 = pawn == null ? string.Empty : "\n\n" + "SingleVisitorArrivesLeaderInfo".Translate(list[0].Named("PAWN")).AdjustedFor(list[0]);
                letterLabel = "LetterLabelSingleVisitorArrives".Translate();
                letterText = "SingleVisitorArrives".Translate(list[0].story.Title, factionResult.Name, list[0].Name.ToStringFull, value, value2, list[0].Named("PAWN")).AdjustedFor(list[0]);
            }
            else {
                string value3 = string.Empty;
                string value4 = pawn == null ? string.Empty : "\n\n" + "GroupVisitorsArriveLeaderInfo".Translate(pawn.LabelShort, pawn);
                letterLabel = "LetterLabelGroupVisitorsArrive".Translate();
                letterText = "GroupVisitorsArrive".Translate(factionResult.Name, value3, value4);
            }
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true);
            SendLetter(allParams, letterLabel, letterText, LetterDefOf.PositiveEvent, null);
            return ir;
        }

        private static IEnumerable<Faction> CandidateFactions(Map map, bool desperate = false) {
            return from f in Find.FactionManager.AllFactions
                where FactionCanBeGroupSource2(f, map, desperate)
                select f;
        }

        private static bool FactionCanBeGroupSource2(Faction f, Map map, bool desperate = false) {
            return FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) &&
                   !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f);
        }

        private static bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false) {
            if (f.IsPlayer) {
                return false;
            }

            if (f.defeated) {
                return false;
            }

            return desperate || f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) && f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp);
        }
        
        private List<Pawn> SpawnPawns(Map map, IncidentParms fakeParams)
        {
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Peaceful, fakeParams, true);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, false).ToList();
            foreach (Pawn item in list)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(fakeParams.spawnCenter, map, 5);
                GenSpawn.Spawn(item, loc, map);
            }
            return list;
        }
    }

    public class HumanIncidentParams_VisitorGroup : HumanIncidentParms {
        public List<String> Names;

        public HumanIncidentParams_VisitorGroup() {
        }

        public HumanIncidentParams_VisitorGroup(String target, HumanLetter letter, List<String> names = null) :
            base(target, letter) {
            Names = names ?? new List<string>();
        }


        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
        }
    }
}