using System;
using HumanStoryteller.Incidents.GameConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TimeTravel : HumanIncidentWorker {
        public const String Name = "TimeTravel";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_TimeTravel)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TimeTravel allParams = Tell.AssertNotNull((HumanIncidentParams_TimeTravel) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            
            Find.TickManager.DebugSetTicksGame(Mathf.RoundToInt(Find.TickManager.TicksGame + 2500 * allParams.HourChange));
            
            if (parms.Letter?.Type != null) {
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }
            return ir;
        }
    }

    public class HumanIncidentParams_TimeTravel : HumanIncidentParms {
        public float HourChange;

        public HumanIncidentParams_TimeTravel() {
        }

        public HumanIncidentParams_TimeTravel(String target, HumanLetter letter, float hourChange = 0) : base(target,
            letter) {
            HourChange = hourChange;
        }

        public override string ToString() {
            return $"{base.ToString()}, HourChange: {HourChange}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref HourChange, "hourChange");
        }
    }
}