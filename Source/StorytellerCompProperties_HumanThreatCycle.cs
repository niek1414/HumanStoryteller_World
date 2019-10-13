using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;

namespace HumanStoryteller {
    public class StorytellerCompProperties_HumanThreatCycle : StorytellerCompProperties {
        public StorytellerCompProperties_HumanThreatCycle() {
            compClass = typeof(StorytellerComp_HumanThreatCycle);
            Tell.Log("Init - version " + HumanStoryteller.VERSION + " " + HumanStoryteller.VERSION_NAME);
        }
    }

    public class StorytellerDef_Human : StorytellerDef {
    }
}