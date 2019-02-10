using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using Verse;
using Timer = System.Timers.Timer;

namespace HumanStoryteller {
    public class StorytellerComp_HumanThreatCycle : StorytellerComp {
        protected StorytellerCompProperties_HumanThreatCycle Props =>
            (StorytellerCompProperties_HumanThreatCycle) props;

        private int IntervalsPassed => Find.TickManager.TicksGame / 600; // 1/10 of a day

        private bool _missedLastIncidentCheck;
        private int _consecutiveEventCounter;
        private bool _init;
        private readonly Timer _refreshTimer = new Timer();
        
        public static StoryComponent StoryComponent => Tell.AssertNotNull(Find.World?.GetComponent<StoryComponent>(), nameof(StoryComponent), "StorytellerComp_HumanThreatCycle");
        public static bool HumanStorytellerGame;
        public static bool IsNoStory => StoryComponent.Story == null;
        public static long StoryId =>
            Tell.AssertNotNull(StoryComponent.Story.Id, nameof(StoryComponent.Story.Id), "StorytellerComp_HumanThreatCycle");
        
        public static bool DEBUG => true;

        public static void Tick() {
            HumanStorytellerGame = false;
            if (!DebugSettings.enableStoryteller) return;
            foreach (var comp in Current.Game.storyteller.storytellerComps) {
                if (comp.GetType() == typeof(StorytellerComp_HumanThreatCycle) && Find.TickManager.TicksGame > 0) {
                    ((StorytellerComp_HumanThreatCycle) comp).CycleTick();
                    HumanStorytellerGame = true;
                }
            }

            if (Find.TickManager.TicksGame % 50 != 0) return;
            var buttonDef = DefDatabase<MainButtonDef>.AllDefs.First(x => x.defName == "RateTab");
            buttonDef.buttonVisible = !IsNoStory;
        }

        public void CycleTick() {
            if (!_init) {
                Init();
                _init = true;
            }

            if (StoryComponent.Story == null) return;

            if (Find.TickManager.TicksGame % 600 == 0) {
                _consecutiveEventCounter = 0;
                IncidentLoop();
            } else if (_missedLastIncidentCheck && _consecutiveEventCounter <= 5) {
                _missedLastIncidentCheck = false;
                IncidentLoop();
            }

            void IncidentLoop() {
                foreach (StoryEventNode sen in MakeIntervalIncidents()) {
                    if (sen?.StoryNode?.StoryEvent?.Incident?.Worker != null) {
                        var incident = sen.StoryNode.StoryEvent.Incident;
                        sen.Result = incident.Worker.Execute(incident.Parms);
                    } else {
                        Tell.Warn("Returned a incident that was not defined");
                    }
                }
            }
        }

        public IEnumerable<StoryEventNode> MakeIntervalIncidents() {
            if (HumanStoryteller.InitiateEventUnsafe) {
                _missedLastIncidentCheck = true;
                yield break;
            }
            
            StoryComponent.CurrentNodes.RemoveAll(item => item == null);

            int laneCount = StoryComponent.CurrentNodes.Count;
            string laneInfo = laneCount.ToString();
            if (DEBUG) {
                laneInfo += "\n";
                foreach (StoryEventNode node in StoryComponent.CurrentNodes) {
                    laneInfo += node.StoryNode.StoryEvent.Uuid.Substring(0, 4) + " " + node.StoryNode.StoryEvent.Name +
                                (node.StoryNode.LeftChild == null && node.StoryNode.RightChild == null ? "[DORMANT]" : "");
                    laneInfo += "\n";
                }
            }

            Tell.Log("TickOffset: " + IntervalsPassed + ", Concurrent lanes: " + laneInfo);

            if (laneCount > 15) {
                Tell.Warn("More concurrent lanes then 15, this can hurt performance badly. This is because the storymaker used to much dividers.");
            }

            if (laneCount > 500) {
                Tell.Err("More concurrent lanes then 500, probably unintentionally created by" +
                         " looping over a divider or merging multiple concurrent lanes.\n" +
                         "Unused lanes will be cleared! This means that if the story is updated, you may not be able to continue where you left off if you finished the story before.");
            }

            for (var i = 0; i < laneCount; i++) {
                if (_consecutiveEventCounter > 5) {
                    yield break;
                }

                StoryEventNode currentNode = StoryComponent.CurrentNodes[i];
                if (currentNode == null) {
                    continue;
                }

                StoryNode sn = currentNode.StoryNode;

                if (i > 500) {
                    Tell.Err("Limiting lane check, stopped after 500 lanes. Last node: " + currentNode);
                    break;
                }

                if (sn.Divider) {
                    var left = sn.LeftChild != null ? new StoryEventNode(sn.LeftChild?.Node) : null;
                    var right = sn.RightChild != null ? new StoryEventNode(sn.RightChild?.Node) : null;
                    StoryComponent.CurrentNodes.Add(left);
                    StoryComponent.CurrentNodes[i] = right;
                    _missedLastIncidentCheck = true;
                    yield return left;
                    _missedLastIncidentCheck = true;
                    _consecutiveEventCounter += 2; //Always execute a divider's children together
                    yield return right;
                } else {
                    if (laneCount > 400 && sn.LeftChild == null && sn.RightChild == null) {
                        StoryComponent.CurrentNodes[i] = null;
                    } else {
                        StoryNode newEvent = StoryComponent.Story.StoryGraph.TryNewEvent(currentNode, IntervalsPassed);
                        if (newEvent == null) continue;
                        _missedLastIncidentCheck = true;
                        _consecutiveEventCounter++;
                        StoryComponent.CurrentNodes[i] = new StoryEventNode(newEvent);
                        yield return StoryComponent.CurrentNodes[i];
                    }
                }
            }

            StoryComponent.CurrentNodes.RemoveAll(item => item == null);
        }

        private void CheckStoryRefresh(object source, ElapsedEventArgs e) {
            Storybook.GetStory(StoryComponent.StoryId, GetStoryCallback);
        }

        private void GetStoryCallback(Story story) {
            if (Current.Game == null || StoryComponent == null || !(Find.TickManager.TicksGame > 0)) {
                _refreshTimer.Enabled = false;
                Tell.Warn("Tried to get story while not in-game");
                return;
            }

            if (story == null) {
                _refreshTimer.Enabled = false;
                Messages.Message(Translator.Translate("StoryNotFound"), MessageTypeDefOf.NegativeEvent, false);
                return;
            }

            HumanStoryteller.InitiateEventUnsafe = true;
            Thread.Sleep(1000); //Give some time to finish undergoing event executions
            StoryComponent.Story = story;
            if (StoryComponent.CurrentNodes.Count == 0) {
                StoryComponent.CurrentNodes.Add(new StoryEventNode(StoryComponent.Story.StoryGraph.Root));
            } else {
                for (int i = 0; i < StoryComponent.CurrentNodes.Count; i++) {
                    var foundNode = StoryComponent.Story.StoryGraph.GetCurrentNode(StoryComponent.CurrentNodes[i]?.StoryNode.StoryEvent.Uuid);
                    StoryComponent.CurrentNodes[i] = foundNode == null ? null : new StoryEventNode(foundNode, StoryComponent.CurrentNodes[i].Result);
                }
                StoryComponent.CurrentNodes.RemoveAll(item => item == null);
            }
            HumanStoryteller.InitiateEventUnsafe = false;
        }

        private void Init() {
            Tell.Log("INITIALIZE HS GAME", Current.Game.storyteller.def.listOrder);
            StoryComponent.StoryId = Current.Game.storyteller.def.listOrder;
//            
//            StorytellerDef storyteller = (from d in DefDatabase<StorytellerDef>.AllDefs
//                where d.defName.Contains("Human")
//                select d).First();
//            Tell.Warn("SWITCHED TO TEST STORYTELLER: " + storyteller.defName);
//            Current.Game.storyteller.def = storyteller;
//            Current.Game.storyteller.Notify_DefChanged();
            
            Storybook.GetStory(StoryComponent.StoryId, GetStoryCallback);
            _refreshTimer.Elapsed += CheckStoryRefresh;
            _refreshTimer.Interval = 60000;
            _refreshTimer.Enabled = true;
        }
    }
}