using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SolarFlare : HumanIncidentWorker {
        public const String Name = "SolarFlare";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_SolarFlare)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_SolarFlare
                allParams = Tell.AssertNotNull((HumanIncidentParams_SolarFlare) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDefOf.SolarFlare;
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition gameCondition_SolarFlare =
                GameConditionMaker.MakeCondition(GameConditionDefOf.SolarFlare, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_SolarFlare);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_SolarFlare : HumanIncidentParms {
        public Number Duration = new Number();

        public HumanIncidentParams_SolarFlare() {
        }

        public HumanIncidentParams_SolarFlare(Target target, HumanLetter letter) : base(target, letter) {
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