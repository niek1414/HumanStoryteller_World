using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.Model; 
public class LongStoryGraph : StoryGraph {
    public LongStoryGraph() {
    }

    public LongStoryGraph(string id, StoryNode root) : base(id, root) {
    }

    public override bool IsLongStory() {
        return true;
    }

    public HumanIncidentParams_LongEntry StoryParams() {
        if (Root.StoryEvent.Incident.Params is HumanIncidentParams_LongEntry rootParams) {
            return rootParams;
        }

        Tell.Err("Root event parameters should be of type 'HumanIncidentParams_LongEntry' but is: " + Root.StoryEvent.Incident.Params.GetType());
        return null;
    }
}