using System;
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
        private const int SHORT_REFRESH = 60000 * 2;
        private const int MEDIUM_REFRESH = 60000 * 10;
        private const int LONG_REFRESH = 60000 * 60;
        private const int OFF_REFRESH = Int32.MaxValue;
        private RefreshRate currentRate = RefreshRate.Long;

        public enum RefreshRate {
            Short,
            Medium,
            Long,
            Off
        }

        protected StorytellerCompProperties_HumanThreatCycle Props =>
            (StorytellerCompProperties_HumanThreatCycle) props;

        private int IntervalsPassed => Find.TickManager.TicksGame / 600; // 1/10 of a day

        private bool _missedLastIncidentCheck = true; //Start game with a first check
        private int _consecutiveEventCounter;
        private bool _init;
        private readonly Timer _refreshTimer = new Timer();

        public static StoryComponent StoryComponent => Tell.AssertNotNull(Find.World?.GetComponent<StoryComponent>(), nameof(StoryComponent),
            "StorytellerComp_HumanThreatCycle");

        public static bool HumanStorytellerGame;
        public static bool IsNoStory => StoryComponent.Story == null;

        public static long StoryId =>
            IsNoStory ? -1 : Tell.AssertNotNull(StoryComponent.Story.Id, nameof(StoryComponent.Story.Id), "StorytellerComp_HumanThreatCycle");

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

            var interval =
                Find.TickManager.CurTimeSpeed == TimeSpeed.Superfast ||
                Find.TickManager.CurTimeSpeed == TimeSpeed.Ultrafast
                    ? 600
                    : 300;
            if (Find.TickManager.TicksGame % interval == 0) {
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
                        DataBank.ProcessVariableModifications(sen.StoryNode.Modifications);
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

            if (Prefs.DevMode) {
                Tell.Log("TickOffset: " + IntervalsPassed + ", Concurrent lanes: " + laneInfo);
                Tell.Log("Vars: \n" + DataBank.VariableBankToString());
            }

            if (laneCount > 15) {
                Tell.Warn("More concurrent lanes then 15, this can hurt performance badly. This is because the storymaker used to much dividers.");
            }

            if (laneCount > 500) {
                Tell.Err("More concurrent lanes then 500, probably unintentionally created by" +
                         " looping over a divider or merging multiple concurrent lanes.\n" +
                         "Unused lanes will be cleared! This means that if the story is updated, you may not be able to continue where you left off if you finished the story before.");
            }

            for (var i = 0; i < laneCount; i++) {
                if (_consecutiveEventCounter > 10) {
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
                        if (!newEvent.StoryEvent.Uuid.Equals(currentNode.StoryNode.StoryEvent.Uuid)) {
                            _missedLastIncidentCheck = true;
                            _consecutiveEventCounter++;
                        }

                        StoryComponent.CurrentNodes[i] = new StoryEventNode(newEvent);
                        yield return StoryComponent.CurrentNodes[i];
                    }
                }
            }

            StoryComponent.CurrentNodes.RemoveAll(item => item == null);
        }

        private void CheckStoryRefresh(object source, ElapsedEventArgs e) {
            if (Current.Game == null || StoryComponent == null || !(Find.TickManager.TicksGame > 0)) {
                _refreshTimer.Enabled = false;
                Tell.Warn("Tried to get story while not in-game");
                return;
            }

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
            StoryComponent.AllNodes = StoryComponent.Story.StoryGraph.GetAllNodes();
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
            if (!StoryComponent.Initialised) {
                StoryComponent.Initialised = true;
                var storyId = Current.Game.storyteller.def.listOrder;
                StoryComponent.StoryId = storyId;
                Tell.Log("INITIALIZE HS GAME", storyId);
                Current.Game.Scenario = Current.Game.Scenario.CopyForEditing();
                Tell.Log("COPIED SCENARIO");
            } else {
                Tell.Log("CONTINUING HS GAME", StoryComponent.StoryId);
            }
            StoryComponent.ThreatCycle = this;

            string str = "";
            foreach (var def in from d in DefDatabase<MentalBreakDef>.AllDefs
                select d) {
                str += def.defName + "\n";
            }

            Tell.Log(str);
            str = "";
            foreach (var def in from d in DefDatabase<MentalBreakDef>.AllDefs
                select d) {
                str += def.label + "\n";
            }
            Tell.Log(str);

            Storybook.GetStory(StoryComponent.StoryId, GetStoryCallback);
            _refreshTimer.Elapsed += CheckStoryRefresh;
            _refreshTimer.Interval = LONG_REFRESH;
            _refreshTimer.Enabled = true;
        }

        public RefreshRate CurrentRate => currentRate;

        public void SetRefreshRate(RefreshRate rate) {
            switch (rate) {
                case RefreshRate.Short:
                    _refreshTimer.Interval = SHORT_REFRESH;
                    currentRate = RefreshRate.Short;
                    break;
                case RefreshRate.Medium:
                    _refreshTimer.Interval = MEDIUM_REFRESH;
                    currentRate = RefreshRate.Medium;
                    break;
                case RefreshRate.Long:
                    _refreshTimer.Interval = LONG_REFRESH;
                    currentRate = RefreshRate.Long;
                    break;
                case RefreshRate.Off:
                    _refreshTimer.Interval = OFF_REFRESH;
                    currentRate = RefreshRate.Off;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rate), rate, null);
            }
        }
    }
}