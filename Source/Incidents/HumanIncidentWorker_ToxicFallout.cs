using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ToxicFallout : HumanIncidentWorker {
        public const String Name = "ToxicFallout";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_ToxicFallout)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ToxicFallout
                allParams = Tell.AssertNotNull((HumanIncidentParams_ToxicFallout) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("ToxicFallout");
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_ToxicFallout gameCondition_ToxicFallout =
                (GameCondition_ToxicFallout) GameConditionMaker.MakeCondition(GameConditionDefOf.ToxicFallout, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_ToxicFallout);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_ToxicFallout : HumanIncidentParms {
        public float Duration;

        public HumanIncidentParams_ToxicFallout() {
        }

        public HumanIncidentParams_ToxicFallout(String target, HumanLetter letter, float duration = -1) : base(target,
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