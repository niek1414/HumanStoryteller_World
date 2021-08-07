using System;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.Incidents {
    public abstract class HumanIncidentWorker_Exit : HumanIncidentWorker {
        protected override IncidentResult Execute(HumanIncidentParams @params) {
            Tell.Err("Exit objects should NEVER be executed!");
            throw new InvalidOperationException("HS_ ShortExit object should NEVER be executed!");
        }
    }
    
    public abstract class HumanIncidentParams_Exit : HumanIncidentParams {
    }
}