using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SolarFlare : HumanIncidentWorker {
        public const String Name = "SolarFlare";

        public override IncidentResult Execute(HumanIncidentParms parms) {
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
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition gameCondition_SolarFlare =
                GameConditionMaker.MakeCondition(GameConditionDefOf.SolarFlare, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_SolarFlare);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_SolarFlare : HumanIncidentParms {
        public float Duration;

        public HumanIncidentParams_SolarFlare() {
        }

        public HumanIncidentParams_SolarFlare(String target, HumanLetter letter, float duration = -1) : base(target,
            letter) {
            Duration = duration;
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Duration, "duration");
        }
    }
}