using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
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
        public Number HourChange = new Number(0);

        public HumanIncidentParams_TimeTravel() {
        }

        public HumanIncidentParams_TimeTravel(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, HourChange: [{HourChange}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref HourChange, "hourChange");
        }
    }
}