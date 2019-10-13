using HumanStoryteller.Incidents;
using Verse;

namespace HumanStoryteller.Model.StoryPart {
    public class StoryEventNode : IExposable {
        private StoryNode _storyNode;
        private int _executeTick;
        private IncidentResult _result;

        public StoryEventNode() {
        }

        public StoryEventNode(StoryNode storyNode, int executeTick, IncidentResult result = null) {
            _storyNode = storyNode;
            _executeTick = executeTick;
            _result = result;
        }

        public StoryNode StoryNode => _storyNode;
        public int ExecuteTick => _executeTick;

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
            Scribe_Values.Look(ref _executeTick, "executeTick");
        }
    }
}