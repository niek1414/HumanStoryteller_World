using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RefugeeChased : HumanIncidentWorker {
        public const String Name = "RefugeeChased";

        private static readonly IntRange RaidDelay = new IntRange(3000, 6000);

        private static readonly FloatRange RaidPointsFactorRange = new FloatRange(1f, 1.6f);

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_RefugeeChased)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_RefugeeChased
                allParams = Tell.AssertNotNull((HumanIncidentParams_RefugeeChased) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();


            if (!TryFindSpawnSpot(map, out IntVec3 spawnSpot)) {
                return ir;
            }

            if (!TryFindEnemyFaction(out Faction enemyFac)) {
                return ir;
            }

            IncidentResult_Dialog irDialog = new IncidentResult_Dialog(null);
            int @int = Rand.Int;
            IncidentParms raidParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, map);
            var paramsPoints = allParams.Points.GetValue();
            float points = paramsPoints >= 0 ? raidParms.points * paramsPoints : raidParms.points;
            raidParms.forced = true;
            raidParms.faction = enemyFac;
            raidParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            raidParms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            raidParms.spawnCenter = spawnSpot;
            raidParms.points = Mathf.Max(points * RaidPointsFactorRange.RandomInRange,
                enemyFac.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            raidParms.pawnGroupMakerSeed = @int;

            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Combat, raidParms);
            defaultPawnGroupMakerParms.points = IncidentWorker_Raid.AdjustedRaidPoints(defaultPawnGroupMakerParms.points, raidParms.raidArrivalMode,
                raidParms.raidStrategy, defaultPawnGroupMakerParms.faction, PawnGroupKindDefOf.Combat);
            IEnumerable<PawnKindDef> pawnKinds = PawnGroupMakerUtility.GeneratePawnKindsExample(defaultPawnGroupMakerParms);
            Pawn refugee = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.SpaceRefugee,
                DownedRefugeeQuestUtility.GetRandomFactionForRefugee(), PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, false,
                20f));
            refugee.relations.everSeenByPlayer = true;
            PawnUtil.SavePawnByName(allParams.OutName, refugee);

            TaggedString text = "RefugeeChasedInitial".Translate(refugee.Name.ToStringFull, refugee.story.Title, enemyFac.def.pawnsPlural, enemyFac.Name,
                refugee.ageTracker.AgeBiologicalYears, PawnUtility.PawnKindsToCommaList(pawnKinds, true), refugee.Named("PAWN"));
            text = text.AdjustedFor(refugee);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, refugee);

            string title = "RefugeeChasedTitle".Translate(map.Parent.Label);
            if (@params.Letter?.Type != null) {
                if (@params.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }

                title = @params.Letter.Title.Get();
                text = @params.Letter.Message.Get();
            }

            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("RefugeeChasedInitial_Accept".Translate());
            diaOption.action = delegate {
                irDialog.LetterAnswer = DialogResponse.Accepted;
                GenSpawn.Spawn(refugee, spawnSpot, map);
                refugee.SetFaction(Faction.OfPlayer);
                CameraJumper.TryJump(refugee);
                QueuedIncident qi = new QueuedIncident(new FiringIncident(IncidentDefOf.RaidEnemy, null, raidParms),
                    Find.TickManager.TicksGame + RaidDelay.RandomInRange);
                Find.Storyteller.incidentQueue.Add(qi);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            string text2 = "RefugeeChasedRejected".Translate(refugee.LabelShort, refugee);
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate());
            diaOption2.resolveTree = true;
            diaNode2.options.Add(diaOption2);

            DiaOption diaOption3 = new DiaOption("RefugeeChasedInitial_Reject".Translate());
            diaOption3.action = delegate {
                irDialog.LetterAnswer = DialogResponse.Denied;
                Find.WorldPawns.PassToWorld(refugee);
            };
            diaOption3.link = diaNode2;
            diaNode.options.Add(diaOption3);
            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, enemyFac, true, true, title));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, title, enemyFac));
            return ir;
        }

        private bool TryFindSpawnSpot(Map map, out IntVec3 spawnSpot) {
            Predicate<IntVec3> validator = c => map.reachability.CanReachColony(c) && !c.Fogged(map);
            return CellFinder.TryFindRandomEdgeCellWith(validator, map, CellFinder.EdgeRoadChance_Neutral, out spawnSpot);
        }

        private bool TryFindEnemyFaction(out Faction enemyFac) {
            return (from f in Find.FactionManager.AllFactions
                where !f.def.hidden && !f.defeated && f.HostileTo(Faction.OfPlayer)
                select f).TryRandomElement(out enemyFac);
        }
    }

    public class HumanIncidentParams_RefugeeChased : HumanIncidentParams {
        public Number Points = new Number();
        public string OutName = "";

        public HumanIncidentParams_RefugeeChased() {
        }

        public HumanIncidentParams_RefugeeChased(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Points: [{Points}], Name: [{OutName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Values.Look(ref OutName, "name");
        }
    }
}