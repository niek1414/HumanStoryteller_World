using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PsychicSoothe : HumanIncidentWorker {
        public const String Name = "PsychicSoothe";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_PsychicSoothe)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_PsychicSoothe
                allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicSoothe) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : IncidentDef.Named("PsychicSoothe").durationDays.RandomInRange * 60000f);
            GameCondition_PsychicEmanation gameCondition_PsychicEmanation =
                (GameCondition_PsychicEmanation) GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration);

            Gender g = PawnUtil.GetGender(allParams.Gender);

            gameCondition_PsychicEmanation.gender = g != Gender.None ? g : map.mapPawns.FreeColonists.RandomElement().gender;
            map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
            string text = "LetterIncidentPsychicSoothe".Translate(g.ToString().Translate().ToLower());
            SendLetter(allParams, "LetterLabelPsychicSoothe".Translate(), text, LetterDefOf.PositiveEvent, null);

            return ir;
        }
    }

    public class HumanIncidentParams_PsychicSoothe : HumanIncidentParams {
        public Number Duration = new Number();
        public string Gender = "";

        public HumanIncidentParams_PsychicSoothe() {
        }

        public HumanIncidentParams_PsychicSoothe(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: [{Duration}], Gender: [{Gender}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Values.Look(ref Gender, "gender");
        }
    }
}