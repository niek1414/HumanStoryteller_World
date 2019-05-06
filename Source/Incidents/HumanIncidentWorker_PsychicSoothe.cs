using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PsychicSoothe : HumanIncidentWorker {
        public const String Name = "PsychicSoothe";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PsychicSoothe)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PsychicSoothe
                allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicSoothe) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : IncidentDef.Named("PsychicSoothe").durationDays.RandomInRange * 60000f);
            GameCondition_PsychicEmanation gameCondition_PsychicEmanation =
                (GameCondition_PsychicEmanation) GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration, 0);

            Gender g = PawnUtil.GetGender(allParams.Gender);

            gameCondition_PsychicEmanation.gender = g != Gender.None ? g : map.mapPawns.FreeColonists.RandomElement().gender;
            map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
            string text = "LetterIncidentPsychicSoothe".Translate(g.ToString().Translate().ToLower());
            SendLetter(allParams, "LetterLabelPsychicSoothe".Translate(), text, LetterDefOf.PositiveEvent, null);

            return ir;
        }
    }

    public class HumanIncidentParams_PsychicSoothe : HumanIncidentParms {
        public Number Duration;
        public string Gender;

        public HumanIncidentParams_PsychicSoothe() {
        }

        public HumanIncidentParams_PsychicSoothe(String target, HumanLetter letter, String gender = "") : this(target, letter, new Number(), gender) {
        }

        public HumanIncidentParams_PsychicSoothe(string target, HumanLetter letter, Number duration, string gender) : base(target, letter) {
            Duration = duration;
            Gender = gender;
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}, Gender: {Gender}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Values.Look(ref Gender, "gender");
        }
    }
}