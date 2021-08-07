using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_VolcanicWinter : HumanIncidentWorker {
        public const String Name = "VolcanicWinter";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_VolcanicWinter)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_VolcanicWinter
                allParams = Tell.AssertNotNull((HumanIncidentParams_VolcanicWinter) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("VolcanicWinter");
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_VolcanicWinter gameCondition_VolcanicWinter =
                (GameCondition_VolcanicWinter) GameConditionMaker.MakeCondition(GameConditionDefOf.VolcanicWinter, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_VolcanicWinter);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_VolcanicWinter : HumanIncidentParams {
        public Number Duration = new Number();

        public HumanIncidentParams_VolcanicWinter() {
        }

        public HumanIncidentParams_VolcanicWinter(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: [{Duration}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
        }
    }
}