using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class AudioCheck : CheckCondition {
        public const String Name = "Audio";


        public AudioCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Err($"Tried to check {GetType()} but result type was null." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            if (!(result is IncidentResult_Audio)) {
                Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_Audio resultAudio = (IncidentResult_Audio) result;
            return resultAudio.EndAfter != -1 && resultAudio.EndAfter < RealTime.LastRealTime;
        }
    }

    public class IncidentResult_Audio : IncidentResult {
        public float EndAfter = -1;

        public IncidentResult_Audio() {
        }

        public IncidentResult_Audio(long endAfter) {
            EndAfter = endAfter;
        }

        public override string ToString() {
            return $"{base.ToString()}, EndAfter: {EndAfter}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref EndAfter, "endAfter");
        }
    }
}