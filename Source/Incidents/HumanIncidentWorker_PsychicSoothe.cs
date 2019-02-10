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

        public override IncidentResult Execute(HumanIncidentParms parms) {
            if (!(parms is HumanIncidentParams_PsychicSoothe)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return null;
            }

            HumanIncidentParams_PsychicSoothe
                allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicSoothe) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            int duration = (int) (allParams.Duration != -1
                ? allParams.Duration * 60000f
                : IncidentDef.Named("PsychicSoothe").durationDays.RandomInRange * 60000f);
            GameCondition_PsychicEmanation gameCondition_PsychicEmanation =
                (GameCondition_PsychicEmanation) GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicSoothe, duration, 0);

            Gender g;
            switch (allParams.Gender) {
                case "MALE":
                    g = Gender.Male;
                    break;
                case "FEMALE":
                    g = Gender.Female;
                    break;
                default:
                    g = Gender.None;
                    break;
            }

            gameCondition_PsychicEmanation.gender = g != Gender.None ? g : map.mapPawns.FreeColonists.RandomElement().gender;
            map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
            string text = "LetterIncidentPsychicSoothe".Translate(g.ToString().Translate().ToLower());
            SendLetter(allParams, "LetterLabelPsychicSoothe".Translate(), text, LetterDefOf.PositiveEvent, null);
            
            return null;
        }
    }

    public class HumanIncidentParams_PsychicSoothe : HumanIncidentParms {
        public float Duration;
        public string Gender;

        public HumanIncidentParams_PsychicSoothe() {
        }

        public HumanIncidentParams_PsychicSoothe(String target, HumanLetter letter, float duration = -1, String gender = "") : base(target,
            letter) {
            Duration = duration;
            Gender = gender;
        }

        public override string ToString() {
            return $"{base.ToString()}, Duration: {Duration}, Gender: {Gender}";
        }
        
        public override void ExposeData() {
             base.ExposeData();
             Scribe_Values.Look(ref Duration, "duration");
             Scribe_Values.Look(ref Gender, "gender");
         }
    }
}