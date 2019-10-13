using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class TraveledCheck : CheckCondition {
        public const String Name = "Traveled";


        public TraveledCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Err($"Tried to check {GetType()} but result type was null.");
                return false;
            }

            if (!(result is IncidentResult_Traveled)) {
                Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_Traveled resultTraveled = (IncidentResult_Traveled) result;
            return resultTraveled.HasTraveled();
        }
    }

    public class IncidentResult_Traveled : IncidentResult {
        private bool _reached;

        public IncidentResult_Traveled() {
        }

        public void Traveled() {
            _reached = true;
        }

        public bool HasTraveled() {
            return _reached;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _reached, "reached");
        }
    }
}