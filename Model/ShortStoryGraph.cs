using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model; 
public class ShortStoryGraph : StoryGraph {
    public bool InPool = true;

    public ShortStoryGraph() {
    }

    public ShortStoryGraph(string id, StoryNode root) : base(id, root) {
    }

    public override bool IsShortStory() {
        return true;
    }

    public HumanIncidentParams_ShortEntry StoryParams() {
        if (Root.StoryEvent.Incident.Params is HumanIncidentParams_ShortEntry rootParams) {
            return rootParams;
        }

        Tell.Err("Root event parameters should be of type 'HumanIncidentParams_ShortEntry' but is: " + Root.StoryEvent.Incident.Params.GetType());
        return null;
    }
    
    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref InPool, "inPool");
    }
}