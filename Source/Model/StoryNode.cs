using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Model {
    public class StoryNode : IExposable {
        private StoryEvent _storyEvent;
        private Connection _leftChild;
        private Connection _rightChild;
        private List<CheckCondition> _conditions;
        public bool Divider;

        public StoryNode() {
        }

        public StoryNode(StoryEvent storyEvent, Connection leftChild = null, Connection rightChild = null,
            List<CheckCondition> conditions = null) {
            _storyEvent = Tell.AssertNotNull(storyEvent, nameof(storyEvent), GetType().Name);
            _leftChild = leftChild;
            _rightChild = rightChild;
            _conditions = conditions;
        }

        public StoryEvent StoryEvent => _storyEvent;

        public Connection LeftChild {
            get => _leftChild;
            set => _leftChild = value;
        }

        public Connection RightChild {
            get => _rightChild;
            set => _rightChild = value;
        }

        public List<CheckCondition> Conditions {
            get => _conditions;
            set => _conditions = value;
        }

        public override string ToString() {
            return $"StoryEvent: {_storyEvent}, LeftChild: {_leftChild}, RightChild: {_rightChild}, Conditions: {_conditions}";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref _storyEvent, "storyEvent");
            Scribe_Deep.Look(ref _leftChild, "leftChild");
            Scribe_Deep.Look(ref _rightChild, "rightChild");
            Scribe_Values.Look(ref Divider, "divider");
            Scribe_Collections.Look(ref _conditions, "condition", LookMode.Deep);
        }
    }
}