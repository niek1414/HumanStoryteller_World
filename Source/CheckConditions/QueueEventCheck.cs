using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class QueueEventCheck : CheckCondition {
        public const String Name = "QueueEvent";


        public QueueEventCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Warn($"Tried to check {GetType()} but result type was null.");
                return false;
            }

            if (!(result is IncidentResult_QueueEvent)) {
                Tell.Warn($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_QueueEvent resultQueueEvent = (IncidentResult_QueueEvent) result;
            return resultQueueEvent.HasQueueEvent();
        }
    }

    public class IncidentResult_QueueEvent : IncidentResult {
        private bool _fired;

        public IncidentResult_QueueEvent() {
        }

        public void QueueEventFired() {
            _fired = true;
        }

        public bool HasQueueEvent() {
            return _fired;
        }

        public override string ToString() {
            return $"{base.ToString()}, Fired: {_fired}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _fired, "fired");
        }
    }
}