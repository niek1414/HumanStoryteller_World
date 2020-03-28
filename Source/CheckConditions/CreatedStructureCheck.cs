using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class CreatedStructureCheck : CheckCondition {
        public const String Name = "CreatedStructure";


        public CreatedStructureCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Err($"Tried to check {GetType()} but result type was null.");
                return false;
            }

            if (!(result is IncidentResult_CreatedStructure)) {
                Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_CreatedStructure resultCreatedStructure = (IncidentResult_CreatedStructure) result;
            return resultCreatedStructure.HasCreatedStructure();
        }
    }

    public class IncidentResult_CreatedStructure : IncidentResult {
        private bool _created;

        public IncidentResult_CreatedStructure() {
        }

        public void CreatedStructure() {
            _created = true;
        }

        public bool HasCreatedStructure() {
            return _created;
        }

        public override string ToString() {
            return $"{base.ToString()}, Created: {_created}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _created, "reached");
        }
    }
}