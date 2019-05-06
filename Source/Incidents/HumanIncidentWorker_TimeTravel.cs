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

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_TimeTravel)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TimeTravel allParams = Tell.AssertNotNull((HumanIncidentParams_TimeTravel) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");


            Find.TickManager.DebugSetTicksGame(Mathf.RoundToInt(Find.TickManager.TicksGame + 2500 * allParams.HourChange.GetValue()));

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_TimeTravel : HumanIncidentParms {
        public Number HourChange;

        public HumanIncidentParams_TimeTravel() {
        }

        public HumanIncidentParams_TimeTravel(String target, HumanLetter letter, Number hourChange) : base(target, letter) {
            HourChange = hourChange;
        }

        public HumanIncidentParams_TimeTravel(string target, HumanLetter letter) : this(target, letter, new Number(0)) {
        }

        public override string ToString() {
            return $"{base.ToString()}, HourChange: {HourChange}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref HourChange, "hourChange");
        }
    }
}