using HumanStoryteller.Incidents;
using Verse;

namespace HumanStoryteller.Model {
    public class StoryEventNode : IExposable {
        private StoryNode _storyNode;
        private IncidentResult _result;

        public StoryEventNode() {
        }

        public StoryEventNode(StoryNode storyNode, IncidentResult result = null) {
            _storyNode = storyNode;
            _result = result;
        }

        public StoryNode StoryNode => _storyNode;

        public IncidentResult Result {
            get => _result;
            set => _result = value;
        }

        public override string ToString() {
            return $"StoryNode: {_storyNode}, Result: {_result}";
        }

        public void ExposeData() {
            Scribe_References.Look(ref _storyNode, "storyNode");
            Scribe_Deep.Look(ref _result, "result");
        }
    }
}