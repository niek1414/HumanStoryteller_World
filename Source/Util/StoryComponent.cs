using System.Collections.Generic;
using HumanStoryteller.Model;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Util {
    public class StoryComponent : WorldComponent {
        public Story Story;
        public long StoryId;
        public List<StoryEventNode> CurrentNodes = new List<StoryEventNode>();
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Story, "story");
            Scribe_Values.Look(ref StoryId, "storyId");
            Scribe_Collections.Look(ref CurrentNodes, "currentNode", LookMode.Deep);
        }

        public StoryComponent(World world) : base(world) {
            Tell.Log("StoryComponent (Re)created", this);
        }
    }
}