using System;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Planetkiller : HumanIncidentWorker {
        public const String Name = "Planetkiller";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_Planetkiller)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_Planetkiller
                allParams = Tell.AssertNotNull((HumanIncidentParams_Planetkiller) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var def = GameConditionDef.Named("Planetkiller");
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : (from x in DefDatabase<ScenPartDef>.AllDefs
                where x.defName == "GameCondition_Planetkiller"
                select x).First().durationRandomRange.RandomInRange * 60000f);
            GameCondition_Planetkiller gameCondition_Planetkiller =
                (GameCondition_Planetkiller) GameConditionMaker.MakeCondition(def, duration);
            map.gameConditionManager.RegisterCondition(gameCondition_Planetkiller);
            SendLetter(allParams, def.label, def.description, LetterDefOf.NegativeEvent, null);
            return ir;
        }
    }

    public class HumanIncidentParams_Planetkiller : HumanIncidentParams {
        public Number Duration = new Number();

        public HumanIncidentParams_Planetkiller() {
        }

        public HumanIncidentParams_Planetkiller(Target target, HumanLetter letter) : base(target, letter) {
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