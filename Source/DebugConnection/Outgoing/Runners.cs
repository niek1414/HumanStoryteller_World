using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model.StoryPart;

namespace HumanStoryteller.DebugConnection.Outgoing {
    public class Runners : Message {
        public List<string> UuidList { get; set; }

        public Runners() : base(MessageType.Runners) {
        }

        public Runners(List<StoryEventNode> current) : this() {
            UuidList = current.Select(node => node.StoryNode.StoryEvent.Uuid).ToList();
        }

        public override string ToString() {
            return $"UuidList: [{UuidList}]";
        }
    }
}