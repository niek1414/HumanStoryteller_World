using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_VolcanicWinter : HumanIncidentWorker {
        public const String Name = "VolcanicWinter";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_VolcanicWinter)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_VolcanicWinter
                allParams = Tell.AssertNotNull((HumanIncidentParams_VolcanicWinter) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = IncidentDef.Named("VolcanicWinter");
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : def.durationDays.RandomInRange * 60000f);
            GameCondition_VolcanicWinter gameCondition_VolcanicWinter =
                (GameCondition_VolcanicWinter) GameConditionMaker.MakeCondition(GameConditionDefOf.VolcanicWinter, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_VolcanicWinter);
            SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
            return ir;
        }
    }

    public class HumanIncidentParams_VolcanicWinter : HumanIncidentParms {
        public float Duration;

        public HumanIncidentParams_VolcanicWinter() {
        }

        public HumanIncidentParams_VolcanicWinter(String target, HumanLetter letter, float duration = -1) : base(target,
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