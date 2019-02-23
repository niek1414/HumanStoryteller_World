using System;
using HumanStoryteller.Incidents.GameConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TempFlux : HumanIncidentWorker {
        public const String Name = "TempFlux";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_TempFlux)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TempFlux allParams = Tell.AssertNotNull((HumanIncidentParams_TempFlux) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            IncidentDef def = IncidentDef.Named(allParams.TempChange < 0 ? "ColdSnap" : "HeatWave");
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            HumanGameCondition_TempFlux tempFlux =
                (HumanGameCondition_TempFlux) GameConditionMaker.MakeCondition(GameConditionDef.Named("TempFlux"), duration);
            tempFlux.MaxTempOffset = allParams.TempChange;
            map.gameConditionManager.RegisterCondition(tempFlux);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_TempFlux : HumanIncidentParms {
        public float Duration;
        public float TempChange;

        public HumanIncidentParams_TempFlux() {
        }

        public HumanIncidentParams_TempFlux(String target, HumanLetter letter, float duration = -1, float tempChange = -20) : base(target,
            letter) {
            Duration = duration;
            TempChange = tempChange;
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}, TempChange: {TempChange}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Duration, "duration");
            Scribe_Values.Look(ref TempChange, "tempChange");
        }
    }
}