using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Research : HumanIncidentWorker {
        public const String Name = "Research";


        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Research)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Research allParams = Tell.AssertNotNull((HumanIncidentParams_Research) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            if (allParams.FinishCurrent) {
                Current.Game.researchManager.FinishProject(Current.Game.researchManager.currentProj);
            }
            
            foreach (var project in allParams.Projects) {
                Current.Game.researchManager.FinishProject(ResearchProjectDef.Named(project));
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_Research : HumanIncidentParms {
        public List<string> Projects = new List<string>();
        public bool FinishCurrent;

        public HumanIncidentParams_Research() {
        }

        public HumanIncidentParams_Research(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Projects, "projects", LookMode.Value);
            Scribe_Values.Look(ref FinishCurrent, "finishCurrent");
        }
    }
}