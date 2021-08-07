using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ToxicFallout : HumanIncidentWorker {
        public const String Name = "ToxicFallout";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_ToxicFallout)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_ToxicFallout
                allParams = Tell.AssertNotNull((HumanIncidentParams_ToxicFallout) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("ToxicFallout");
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_ToxicFallout gameCondition_ToxicFallout =
                (GameCondition_ToxicFallout) GameConditionMaker.MakeCondition(GameConditionDefOf.ToxicFallout, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_ToxicFallout);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_ToxicFallout : HumanIncidentParams {
        public Number Duration = new Number();

        public HumanIncidentParams_ToxicFallout() {
        }

        public HumanIncidentParams_ToxicFallout(Target target, HumanLetter letter) : base(target, letter) {
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