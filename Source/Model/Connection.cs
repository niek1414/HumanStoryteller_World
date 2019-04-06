using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Model {
    public class Connection : IExposable {
        public long Offset;
        public StoryNode Node;

        public Connection() {
        }

        public Connection(long offset, StoryNode node) {
            Offset = Tell.AssertNotNull(offset, nameof(offset), GetType().Name);
            Node = Tell.AssertNotNull(node, nameof(node), GetType().Name);
        }

        public override string ToString() {
            return $"Offset: {Offset}, Node: {Node}";
        }
        
        public void ExposeData() {
            Scribe_References.Look(ref Node, "node");
            Scribe_Values.Look(ref Offset, "offset");
        }
    }
}