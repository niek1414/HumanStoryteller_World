using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Research : HumanIncidentWorker {
        public const String Name = "Research";


        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();
            if (!(@params is HumanIncidentParams_Research)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_Research allParams = Tell.AssertNotNull((HumanIncidentParams_Research) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            if (allParams.FinishCurrent) {
                Current.Game.researchManager.FinishProject(Current.Game.researchManager.currentProj);
            }
            
            foreach (var project in allParams.Projects) {
                Current.Game.researchManager.FinishProject(ResearchProjectDef.Named(project));
            }

            SendLetter(@params);

            return ir;
        }
    }

    public class HumanIncidentParams_Research : HumanIncidentParams {
        public List<string> Projects = new List<string>();
        public bool FinishCurrent;

        public HumanIncidentParams_Research() {
        }

        public HumanIncidentParams_Research(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Projects: [{Projects.ToCommaList()}], FinishCurrent: [{FinishCurrent}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Projects, "projects", LookMode.Value);
            Scribe_Values.Look(ref FinishCurrent, "finishCurrent");
        }
    }
}