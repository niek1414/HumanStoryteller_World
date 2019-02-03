using System.Collections.Generic;
using HumanStoryteller.Model;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Util {
    public class StoryComponent : WorldComponent {
        public Story Story;
        public List<StoryNode> CurrentNodes = new List<StoryNode>();
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Story, "story");
            Scribe_Collections.Look(ref CurrentNodes, "currentNode", LookMode.Deep);
        }

        public StoryComponent(World world) : base(world) {
        }
    }
}