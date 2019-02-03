using HumanStoryteller.Util;
using RimWorld;

namespace HumanStoryteller {
    public class StorytellerCompProperties_HumanThreatCycle : StorytellerCompProperties {
        public StorytellerCompProperties_HumanThreatCycle() {
            compClass = typeof(StorytellerComp_HumanThreatCycle);
            Tell.Log("Property cycle init");
        }

        //TODO add config??
    }

    public class StorytellerDef_Human : StorytellerDef {
        //TODO add config??
    }
}