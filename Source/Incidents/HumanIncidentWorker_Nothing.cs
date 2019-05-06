using System;
using HumanStoryteller.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Nothing : HumanIncidentWorker {
        public const String Name = "Nothing";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            //What? Did you expect a huge file or something?
            
            SendLetter(parms);
            return ir;
        }
    }
}