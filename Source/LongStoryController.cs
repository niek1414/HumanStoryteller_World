using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller {
    public class LongStoryController : IExposable {
        private LongStoryGraph _story;

        public bool HasLongStory() {
            return _story != null;
        }

        public LongStoryGraph Story {
            get => _story;
            set {
                if (_story != null) {
                    Tell.Err("LongStoryController received a new story but an other story was still present. Are there multiple long stories?");
                }

                _story = value;
            }
        }

        public LongStoryController() {
        }

        public HumanIncidentParams_LongEntry StoryParams() {
            return _story?.StoryParams();
        }

        public void Tick() {
            if (Find.TickManager.TicksGame % 500 == 0) {
                Tell.Log("TickOffset: " + StorytellerComp_HumanThreatCycle.IP + ", Graph: LongStory, Concurrent lanes: " +
                         _story.CurrentNodeCount());
            }

            var longStoryInterval =
                Find.TickManager.CurTimeSpeed == TimeSpeed.Superfast ||
                Find.TickManager.CurTimeSpeed == TimeSpeed.Ultrafast
                    ? 30
                    : 15;
            StoryGraphWalkerUtil.TickGraph(_story, longStoryInterval);
        }

        public void PreLoad() {
            if (!HasLongStory()) return;
            var allNodes = _story.GetAllNodes();
            foreach (var node in allNodes) {
                node.StoryEvent.Incident.Worker.PreLoad(node.StoryEvent.Incident.Params);
            }
        }

        public void UpdateCurrentNodes(StoryArc oldStoryArc) {
            if (!HasLongStory()) return;
            if (oldStoryArc?.LongStoryController != null) {
                _story.UpdateCurrentNodes(oldStoryArc.LongStoryController.CurrentNodes());
            }

            if (_story.CurrentNodeCount() == 0) {
                _story.ResetGraph();
            }
        }

        public List<StoryEventNode> CurrentNodes() {
            return _story?.CurrentNodes.ListFullCopy();
        }

        public void TryExecuteEvent(string uuid, bool withRunner) {
            if (!HasLongStory()) return;
            StoryGraphWalkerUtil.TryExecuteNode(_story, uuid, withRunner);
        }

        public override string ToString() {
            return
                $"LongStory: [{_story}]";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref _story, "story");
        }
    }
}