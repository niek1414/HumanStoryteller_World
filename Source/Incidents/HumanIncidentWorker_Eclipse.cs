using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Eclipse : HumanIncidentWorker {
        public const String Name = "Eclipse";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Eclipse)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Eclipse
                allParams = Tell.AssertNotNull((HumanIncidentParams_Eclipse) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("Eclipse");
            var allParamsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(allParamsDuration != -1
                ? allParamsDuration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_Eclipse gameCondition_Eclipse =
                (GameCondition_Eclipse) GameConditionMaker.MakeCondition(GameConditionDefOf.Eclipse, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_Eclipse);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_Eclipse : HumanIncidentParms {
        public Number Duration;

        public HumanIncidentParams_Eclipse() {
        }

        public HumanIncidentParams_Eclipse(String target, HumanLetter letter, Number duration) : base(target,
            letter) {
            Duration = duration;
        }

        public HumanIncidentParams_Eclipse(String target, HumanLetter letter) : this(target, letter, new Number()) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
        }
    }
}