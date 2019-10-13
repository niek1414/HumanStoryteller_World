using System;
using HumanStoryteller.Incidents.GameConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TempFlux : HumanIncidentWorker {
        public const String Name = "TempFlux";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_TempFlux)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TempFlux allParams = Tell.AssertNotNull((HumanIncidentParams_TempFlux) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var tempChange = allParams.TempChange.GetValue();
            IncidentDef def = IncidentDef.Named(tempChange < 0 ? "ColdSnap" : "HeatWave");
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            HumanGameCondition_TempFlux tempFlux =
                (HumanGameCondition_TempFlux) GameConditionMaker.MakeCondition(GameConditionDef.Named("TempFlux"), duration);
            tempFlux.MaxTempOffset = tempChange;
            map.gameConditionManager.RegisterCondition(tempFlux);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_TempFlux : HumanIncidentParms {
        public Number Duration = new Number();
        public Number TempChange = new Number(-20);

        public HumanIncidentParams_TempFlux() {
        }

        public HumanIncidentParams_TempFlux(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}, TempChange: {TempChange}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Deep.Look(ref TempChange, "tempChange");
        }
    }
}