using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PsychicDrone : HumanIncidentWorker {
        public const String Name = "PsychicDrone";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PsychicDrone)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PsychicDrone
                allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicDrone) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            var paramsDuration = allParams.Duration.GetValue();
            int duration = Mathf.RoundToInt(paramsDuration != -1
                ? paramsDuration * 60000f
                : IncidentDef.Named("PsychicDrone").durationDays.RandomInRange * 60000f);
            GameCondition_PsychicEmanation gameCondition_PsychicEmanation =
                (GameCondition_PsychicEmanation) GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration);

            PsychicDroneLevel l;
            switch (allParams.PsyLevel) {
                case "LOW":
                    l = PsychicDroneLevel.BadLow;
                    break;
                case "MEDIUM":
                    l = PsychicDroneLevel.BadMedium;
                    break;
                case "HIGH":
                    l = PsychicDroneLevel.BadHigh;
                    break;
                case "EXTREME":
                    l = PsychicDroneLevel.BadExtreme;
                    break;
                default:
                    l = PsychicDroneLevel.BadMedium;
                    break;
            }

            gameCondition_PsychicEmanation.level = l;

            Gender g = PawnUtil.GetGender(allParams.Gender);

            gameCondition_PsychicEmanation.gender = g != Gender.None ? g : map.mapPawns.FreeColonists.RandomElement().gender;
            map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
            string text = "LetterIncidentPsychicDrone".Translate(g.ToString().Translate().ToLower(), l.GetLabel());
            SendLetter(allParams, "LetterLabelPsychicDrone".Translate(), text, LetterDefOf.NegativeEvent, null);

            return ir;
        }
    }

    public class HumanIncidentParams_PsychicDrone : HumanIncidentParms {
        public Number Duration;
        public string Gender;
        public string PsyLevel;

        public HumanIncidentParams_PsychicDrone() {
        }

        public HumanIncidentParams_PsychicDrone(String target, HumanLetter letter, String gender = "", String psyLevel = "") : this(target, letter, new Number(), gender, psyLevel) {
        }

        public HumanIncidentParams_PsychicDrone(string target, HumanLetter letter, Number duration, string gender, string psyLevel) : base(target,
            letter) {
            Duration = duration;
            Gender = gender;
            PsyLevel = psyLevel;
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}, Gender: {Gender}, PsyLevel: {PsyLevel}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Values.Look(ref Gender, "gender");
            Scribe_Values.Look(ref PsyLevel, "psyLevel");
        }
    }
}