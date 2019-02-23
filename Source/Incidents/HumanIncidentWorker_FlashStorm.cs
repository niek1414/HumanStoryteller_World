using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Flashstorm : HumanIncidentWorker {
        public const String Name = "Flashstorm";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Flashstorm)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Flashstorm
                allParams = Tell.AssertNotNull((HumanIncidentParams_Flashstorm) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("Flashstorm");
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_Flashstorm gameCondition_Flashstorm =
                (GameCondition_Flashstorm) GameConditionMaker.MakeCondition(GameConditionDefOf.Flashstorm, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_Flashstorm);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef,
                new TargetInfo(gameCondition_Flashstorm.centerLocation.ToIntVec3, map));
            if (map.weatherManager.curWeather.rainRate > 0.1f) {
                map.weatherDecider.StartNextWeather();
            }

            return ir;
        }
    }

    public class HumanIncidentParams_Flashstorm : HumanIncidentParms {
        public float Duration;

        public HumanIncidentParams_Flashstorm() {
        }

        public HumanIncidentParams_Flashstorm(String target, HumanLetter letter, float duration = -1) : base(target,
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