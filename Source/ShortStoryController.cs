using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller {
    public class ShortStoryController : IExposable {
        private List<ShortStoryGraph> _stories = new List<ShortStoryGraph>();
        private ShortStoryGraph _activeStory;

        public ShortStoryController() {
        }

        public void AddStory(ShortStoryGraph story) {
            _stories.Add(story);
        }

        public int CountOfInitialStories() {
            return _stories.Count;
        }

        public bool HasActiveStory() {
            return _activeStory != null;
        }

        public void Tick() {
            //TODO do pool stuff
            //After picking, call StoryGraph#ResetGraph

            if (!HasActiveStory()) return;
            var entry = _activeStory.StoryParams();
            if (Find.TickManager.TicksGame % 500 == 0) {
                Tell.Log("TickOffset: " + StorytellerComp_HumanThreatCycle.IP +
                         ", Graph: " + entry.Id + "#" + entry.Name +
                         ", Concurrent lanes: " + _activeStory.CurrentNodeCount());
            }

            var longStoryInterval =
                Find.TickManager.CurTimeSpeed == TimeSpeed.Superfast ||
                Find.TickManager.CurTimeSpeed == TimeSpeed.Ultrafast
                    ? 32
                    : 16;
            var paramsExit = StoryGraphWalkerUtil.TickGraph(_activeStory, longStoryInterval);
            if (paramsExit == null) return;
            
            //TODO use this
            //Recycle back into pool if needed & set a timeout
        }

        public void PreLoad() {
            _stories.ForEach(graph => {
                var allNodes = graph.GetAllNodes();
                foreach (var node in allNodes) {
                    node.StoryEvent.Incident.Worker.PreLoad(node.StoryEvent.Incident.Params);
                }
            });
        }

        public void UpdateCurrentNodes(StoryArc oldStoryArc) {
            // TODO set the active story before setting the currentNodes
            if (!HasActiveStory()) return;
            if (oldStoryArc?.ShortStoryController != null) {
                _activeStory.UpdateCurrentNodes(oldStoryArc.ShortStoryController.CurrentNodes());
            }
        }

        public List<StoryEventNode> CurrentNodes() {
            return _activeStory?.CurrentNodes.ListFullCopy();
        }

        public void TryExecuteEvent(string uuid, bool withRunner) {
            foreach (var graph in _stories) {
                StoryGraphWalkerUtil.TryExecuteNode(graph, uuid, withRunner);
            }
        }

        public override string ToString() {
            return $"ShortStories: [{_stories}]";
        }

        public void ExposeData() {
            Scribe_Collections.Look(ref _stories, "stories", LookMode.Deep);
        }
    }
}