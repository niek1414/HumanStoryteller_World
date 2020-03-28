using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShortCircuit : HumanIncidentWorker {
        public const String Name = "ShortCircuit";

        private static List<IntVec3> _tmpCells = new List<IntVec3>();

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ShortCircuit)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ShortCircuit allParams = Tell.AssertNotNull((HumanIncidentParams_ShortCircuit) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            Building culprit;
            try {
                culprit = ShortCircuitUtility.GetShortCircuitablePowerConduits(map).First();
            } catch (InvalidOperationException) {
                return ir;
            }

            PowerNet powerNet = culprit.PowerComp.PowerNet;
            float totalEnergy = 0f;
            float explosionRadius = 0f;
            bool flag = false;
            if (powerNet.batteryComps.Any(x => x.StoredEnergy > 20f)) {
                DrainBatteriesAndCauseExplosion(powerNet, culprit, out totalEnergy, out explosionRadius);
            } else {
                flag = TryStartFireNear(culprit);
            }

            string value = culprit.def != ThingDefOf.PowerConduit
                ? Find.ActiveLanguageWorker.WithIndefiniteArticlePostProcessed(culprit.Label)
                : "AnElectricalConduit".Translate();
            StringBuilder stringBuilder = new StringBuilder();
            if (flag) {
                stringBuilder.Append("ShortCircuitStartedFire".Translate(value));
            } else {
                stringBuilder.Append("ShortCircuit".Translate(value));
            }

            if (totalEnergy > 0f) {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("ShortCircuitDischargedEnergy".Translate(totalEnergy.ToString("F0")));
            }

            if (explosionRadius > 5f) {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("ShortCircuitWasLarge".Translate());
            }

            if (explosionRadius > 8f) {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine();
                stringBuilder.Append("ShortCircuitWasHuge".Translate());
            }

            SendLetter(allParams, "LetterLabelShortCircuit".Translate(), stringBuilder.ToString(), LetterDefOf.NegativeEvent,
                new TargetInfo(culprit.Position, map));

            return ir;
        }

        private static void DrainBatteriesAndCauseExplosion(PowerNet net, Building culprit, out float totalEnergy, out float explosionRadius) {
            totalEnergy = 0.0f;
            for (int index = 0; index < net.batteryComps.Count; ++index) {
                CompPowerBattery batteryComp = net.batteryComps[index];
                totalEnergy += batteryComp.StoredEnergy;
                batteryComp.DrawPower(batteryComp.StoredEnergy);
            }

            explosionRadius = Mathf.Sqrt(totalEnergy) * 0.05f;
            explosionRadius = Mathf.Clamp(explosionRadius, 1.5f, 14.9f);
            GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius, DamageDefOf.Flame, null);
            if (explosionRadius <= 3.5)
                return;
            GenExplosion.DoExplosion(culprit.Position, net.Map, explosionRadius * 0.3f, DamageDefOf.Bomb, null);
        }

        private static bool TryStartFireNear(Building b) {
            _tmpCells.Clear();
            int num = GenRadial.NumCellsInRadius(3f);
            CellRect startRect = b.OccupiedRect();
            for (int index = 0; index < num; ++index) {
                IntVec3 intVec3 = b.Position + GenRadial.RadialPattern[index];
                if (GenSight.LineOfSight(b.Position, intVec3, b.Map, startRect, CellRect.SingleCell(intVec3)) &&
                    FireUtility.ChanceToStartFireIn(intVec3, b.Map) > 0.0)
                    _tmpCells.Add(intVec3);
            }

            if (_tmpCells.Any())
                return FireUtility.TryStartFireIn(_tmpCells.RandomElement(), b.Map, Rand.Range(0.1f, 1.75f));
            return false;
        }
    }

    public class HumanIncidentParams_ShortCircuit : HumanIncidentParms {
        public HumanIncidentParams_ShortCircuit() {
        }

        public HumanIncidentParams_ShortCircuit(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}