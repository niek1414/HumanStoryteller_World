using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PsychicDrone : HumanIncidentWorker {
        public const String Name = "PsychicDrone";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PsychicDrone)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PsychicDrone
                allParams = Tell.AssertNotNull((HumanIncidentParams_PsychicDrone) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            int duration = Mathf.RoundToInt(allParams.Duration != -1
                ? allParams.Duration * 60000f
                : IncidentDef.Named("PsychicDrone").durationDays.RandomInRange * 60000f);
            GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration);

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
            string text = "LetterIncidentPsychicDrone".Translate(g.ToString().Translate().ToLower(), l.GetLabel());
            SendLetter(allParams, "LetterLabelPsychicDrone".Translate(), text, LetterDefOf.NegativeEvent, null);
            
            return ir;
        }
    }

    public class HumanIncidentParams_PsychicDrone : HumanIncidentParms {
        public float Duration;
        public string Gender;
        public string PsyLevel;

        public HumanIncidentParams_PsychicDrone() {
        }

        public HumanIncidentParams_PsychicDrone(String target, HumanLetter letter, float duration = -1, String gender = "", String psyLevel = "") : base(target,
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
             Scribe_Values.Look(ref Duration, "duration");
             Scribe_Values.Look(ref Gender, "gender");
             Scribe_Values.Look(ref PsyLevel, "psyLevel");
         }
    }
}