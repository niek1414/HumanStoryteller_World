using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Infestation : HumanIncidentWorker {
        public const String Name = "Infestation";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Infestation)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Infestation allParams = Tell.AssertNotNull((HumanIncidentParams_Infestation) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            var paramsPoints = allParams.Points.GetValue();
            float points = paramsPoints >= 0
                ? StorytellerUtility.DefaultThreatPointsNow(map) * paramsPoints
                : StorytellerUtility.DefaultThreatPointsNow(map);
            
            Thing t = SpawnTunnels(Mathf.Max(GenMath.RoundRandom(points / 220f), 1), map);
            IncidentDef def = IncidentDef.Named(Name);
            SendLetter(allParams, def.letterLabel, string.Format(def.letterText).CapitalizeFirst(), def.letterDef, t);
            Find.TickManager.slower.SignalForceNormalSpeedShort();

            return ir;
        }

        private Thing SpawnTunnels(int hiveCount, Map map) {
            if (!InfestationCellFinder.TryFindCell(out IntVec3 cell, map)) {
                return null;
            }

            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
            for (int i = 0; i < hiveCount - 1; i++) {
                cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive,
                    ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), false, true);
                if (cell.IsValid) {
                    thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
                }
            }

            return thing;
        }
    }

    public class HumanIncidentParams_Infestation : HumanIncidentParms {
        public Number Points = new Number();

        public HumanIncidentParams_Infestation() {
        }

        public HumanIncidentParams_Infestation(string target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Points: {Points}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
        }
    }
}