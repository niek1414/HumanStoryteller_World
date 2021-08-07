using System;
using HumanStoryteller.Model.Incident;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShortExit : HumanIncidentWorker_Exit {
        public const String Name = "ShortEntry";
    }

    public class HumanIncidentParams_ShortExit : HumanIncidentParams_Exit {
        public bool BackToPool = true;
        public Number Timeout = new Number();
        
        public HumanIncidentParams_ShortExit() {
        }

        public override string ToString() {
            return $"{base.ToString()}, BackToPool: [{BackToPool}], Timeout: [{Timeout}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref BackToPool, "backToPool");
            Scribe_Values.Look(ref Timeout, "timeout");
        }
    }
}