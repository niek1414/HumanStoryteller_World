using System.Collections.Generic;
using HumanStoryteller.Model;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Util {
    public class StoryComponent : WorldComponent {
        public bool Initialised;
        public Story Story;
        public long StoryId;
        public List<StoryEventNode> CurrentNodes = new List<StoryEventNode>();
        public List<StoryNode> AllNodes = new List<StoryNode>();
        public Dictionary<string, float> VariableBank = new Dictionary<string, float>();
        public Dictionary<string, Pawn> PawnBank = new Dictionary<string, Pawn>();
        public StorytellerComp_HumanThreatCycle ThreatCycle = null;

        private List<string> reservedKeysWorkingList;
        private List<Pawn> reservedValuesWorkingList;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Initialised, "initialised");
            Scribe_Deep.Look(ref Story, "story");
            Scribe_Values.Look(ref StoryId, "storyId");
            Scribe_Collections.Look(ref CurrentNodes, "currentNode", LookMode.Deep);
            Scribe_Collections.Look(ref AllNodes, "allNodes", LookMode.Deep);
            Scribe_Collections.Look(ref VariableBank, "variableBank", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref PawnBank, "pawnBank", LookMode.Value, LookMode.Reference, ref reservedKeysWorkingList, ref reservedValuesWorkingList);
        }

        public StoryComponent(World world) : base(world) {
            Tell.Log("StoryComponent (Re)created", this);
        }
    }
}