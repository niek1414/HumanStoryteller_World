using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RaidEnemy : HumanIncidentWorker {
        public const String Name = "RaidEnemy";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_RaidEnemy)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RaidEnemy allParams = Tell.AssertNotNull((HumanIncidentParams_RaidEnemy) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var paramsPoints = allParams.Points.GetValue();
            float points = paramsPoints >= 0
                ? StorytellerUtility.DefaultThreatPointsNow(map) * paramsPoints
                : StorytellerUtility.DefaultThreatPointsNow(map);
            Faction faction;
            try {
                faction = Find.FactionManager.AllFactions.First(f => f.def.defName == allParams.Faction);
            } catch (InvalidOperationException) {
                if (!PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(points, out faction, f => FactionCanBeGroupSource(f, map, false),
                    true, true, true)) {
                    faction = null;
                }

                if (faction == null && !PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(points, out faction,
                        f => FactionCanBeGroupSource(f, map, true), true, true, true)) {
                    Tell.Err("Found no faction that satisfies requirements; p=" + points + " m=" + map, false);
                    faction = Find.FactionManager.RandomEnemyFaction();
                }
            }

            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;

            IncidentParms fakeParms = new IncidentParms();

            fakeParms.faction = faction;
            fakeParms.points = points;
            fakeParms.target = map;

            RaidStrategyDef strategy;
            try {
                fakeParms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs where d.defName == allParams.Strategy select d).First();
            } catch (InvalidOperationException) {
                if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs
                    where d.Worker.CanUseWith(fakeParms, PawnGroupKindDefOf.Combat) &&
                          (fakeParms.raidArrivalMode != null || d.arriveModes != null && d.arriveModes.Any(x => x.Worker.CanUseWith(fakeParms)))
                    select d).TryRandomElementByWeight(d => d.Worker.SelectionWeight(map, fakeParms.points), out fakeParms.raidStrategy)) {
                    Log.Warning("No raid stategy for " + faction + " with points " + points + ", groupKind=" + PawnGroupKindDefOf.Combat +
                                "\nparms=" +
                                parms);
                    fakeParms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }
            }

            strategy = fakeParms.raidStrategy;
            PawnsArrivalModeDef arriveMode;

            try {
                arriveMode = (from d in DefDatabase<PawnsArrivalModeDef>.AllDefs where d.defName == allParams.ArriveMode select d).First();
            } catch (InvalidOperationException e) {
                if (!(from x in strategy.arriveModes
                    where (x.Worker.def.minTechLevel == TechLevel.Undefined || faction.def.techLevel >= x.Worker.def.minTechLevel)
                    select x).TryRandomElementByWeight(x => x.Worker.def.selectionWeightCurve.Evaluate(points), out arriveMode)) {
                    Tell.Err("Could not resolve arrival mode for raid. Defaulting to EdgeWalkIn.", false);
                    arriveMode = PawnsArrivalModeDefOf.EdgeWalkIn;
                }
            }

            fakeParms.raidArrivalMode = arriveMode;

            if (!arriveMode.Worker.TryResolveRaidSpawnCenter(fakeParms)) {
                Tell.Err("Could not resolve spawn center for raid.");
                fakeParms.spawnCenter = CellFinder.RandomEdgeCell(map);
            }

            points = AdjustedRaidPoints(points, arriveMode, strategy, faction, combat);
            fakeParms.points = points;

            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, fakeParms);
            List<Pawn> list = new List<Pawn>();
            var amount = Mathf.RoundToInt(allParams.Amount.GetValue());
            if (amount == -1) {
                list.AddRange(PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList());
            } else {
                while (amount > list.Count) {
                    list.AddRange(PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList());
                }

                if (amount != list.Count) {
                    list = list.Take(amount).ToList();
                }
            }

            if (list.Count == 0) {
                Tell.Err("Got no pawns spawning raid from parms " + fakeParms, fakeParms);
                return ir;
            }

            arriveMode.Worker.Arrive(list, fakeParms);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Points = " + points.ToString("F0"));
            for (var i = 0; i < list.Count; i++) {
                Pawn p = list[i];
                if (i < allParams.OutNames.Count) {
                    PawnUtil.SavePawnByName(allParams.OutNames[i], p);
                }

                string str = p.equipment == null || p.equipment.Primary == null ? "unarmed" : p.equipment.Primary.LabelCap;
                stringBuilder.AppendLine(p.KindLabel + " - " + str);
            }

            string letterText = GetLetterText(fakeParms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref strategy.letterLabelEnemy, ref letterText,
                "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, faction.def.pawnsPlural), true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (fakeParms.pawnGroups != null) {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, fakeParms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy(x => x.Count);
                if (list4.Any()) {
                    list2.Add(list4[0]);
                }

                for (int i = 0; i < list3.Count; i++) {
                    if (list3[i] != list4 && list3[i].Any()) {
                        list2.Add(list3[i][0]);
                    }
                }
            } else if (list.Any()) {
                list2.Add(list[0]);
            }

            strategy.Worker.MakeLords(fakeParms, list);

            SendLetter(allParams, strategy.letterLabelEnemy, letterText, LetterDefOf.ThreatBig, list2, faction, stringBuilder.ToString());

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts)) {
                for (int j = 0; j < list.Count; j++) {
                    Pawn pawn = list[j];
                    if (pawn.apparel.WornApparel.Any(ap => ap is ShieldBelt)) {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }

            return ir;
        }

        protected string GetLetterText(IncidentParms parms, List<Pawn> pawns) {
            string str = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name);
            str += "\n\n";
            str += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find(x => x.Faction.leader == x);
            if (pawn != null) {
                str += "\n\n";
                str += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }

            return str;
        }

        public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction,
            PawnGroupKindDef groupKind) {
            if (raidArrivalMode.pointsFactorCurve != null) {
                points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
            }

            if (raidStrategy.pointsFactorCurve != null) {
                points *= raidStrategy.pointsFactorCurve.Evaluate(points);
            }

            points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
            return points;
        }

        protected virtual bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false) {
            if (f.IsPlayer) {
                return false;
            }

            if (f.defeated) {
                return false;
            }

            if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) ||
                               !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)) &&
                GenDate.DaysPassed >= f.def.earliestRaidDays) {
                return false;
            }

            if (!f.HostileTo(Faction.OfPlayer)) {
                return false;
            }

            return true;
        }
    }

    public class HumanIncidentParams_RaidEnemy : HumanIncidentParms {
        public Number Points = new Number();
        public Number Amount = new Number();
        public String Faction = "";
        public String Strategy = "";
        public String ArriveMode = "";
        public List<String> OutNames = new List<string>();

        public HumanIncidentParams_RaidEnemy() {
        }

        public HumanIncidentParams_RaidEnemy(string target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return
                $"{base.ToString()}, Points: {Points}, Amount: {Amount}, Faction: {Faction}, Strategy: {Strategy}, ArriveMode: {ArriveMode}, Names: {OutNames}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref Strategy, "strategy");
            Scribe_Values.Look(ref ArriveMode, "arriveMode");
            Scribe_Collections.Look(ref OutNames, "names", LookMode.Value);
        }
    }
}