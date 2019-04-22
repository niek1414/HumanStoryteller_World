﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using Verse;
using Timer = System.Timers.Timer;

namespace HumanStoryteller {
    public class StorytellerComp_HumanThreatCycle : StorytellerComp {
        private HumanStoryteller.RefreshRate currentRate = HumanStoryteller.RefreshRate.Long;

        protected StorytellerCompProperties_HumanThreatCycle Props =>
            (StorytellerCompProperties_HumanThreatCycle) props;

        private int IntervalsPassed => Find.TickManager.TicksGame / 600; // 1/10 of a day

        private bool _missedLastIncidentCheck = true; //Start game with a first check
        private int _consecutiveEventCounter;
        private bool _init;
        public readonly Timer RefreshTimer = new Timer();

        public static void Tick() {
            HumanStoryteller.HumanStorytellerGame = false;
            if (!DebugSettings.enableStoryteller) return;
            foreach (var comp in Current.Game.storyteller.storytellerComps) {
                if (comp.GetType() == typeof(StorytellerComp_HumanThreatCycle) && Find.TickManager.TicksGame > 0) {
                    ((StorytellerComp_HumanThreatCycle) comp).CycleTick();
                    HumanStoryteller.HumanStorytellerGame = true;
                }
            }

            if (Find.TickManager.TicksGame % 50 != 0) return;
            var buttonDef = DefDatabase<MainButtonDef>.AllDefs.First(x => x.defName == "RateTab");
            buttonDef.buttonVisible = !HumanStoryteller.IsNoStory;
        }

        public void CycleTick() {
            if (!_init) {
                Init();
                _init = true;
            } else if (HumanStoryteller.StoryComponent.ForcedUpdate) {
                HumanStoryteller.StoryComponent.Reset();
                Init();
            }

            if (HumanStoryteller.StoryComponent.Story == null) return;

            var interval =
                Find.TickManager.CurTimeSpeed == TimeSpeed.Superfast ||
                Find.TickManager.CurTimeSpeed == TimeSpeed.Ultrafast
                    ? 600
                    : 300;
            if (Find.TickManager.TicksGame % interval == 0) {
                _consecutiveEventCounter = 0;
                IncidentLoop();
            } else if (_missedLastIncidentCheck && _consecutiveEventCounter <= 10) {
                _missedLastIncidentCheck = false;
                IncidentLoop();
            }

            void IncidentLoop() {
                foreach (StoryEventNode sen in MakeIntervalIncidents()) {
                    if (sen?.StoryNode?.StoryEvent?.Incident?.Worker != null) {
                        var incident = sen.StoryNode.StoryEvent.Incident;
                        incident.Worker.PreExecute(incident.Parms);
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

            HumanStoryteller.StoryComponent.CurrentNodes.RemoveAll(item => item == null);

            int laneCount = HumanStoryteller.StoryComponent.CurrentNodes.Count;
            string laneInfo = laneCount.ToString();
            if (HumanStoryteller.DEBUG) {
                laneInfo += "\n";
                foreach (StoryEventNode node in HumanStoryteller.StoryComponent.CurrentNodes) {
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

                StoryEventNode currentNode = HumanStoryteller.StoryComponent.CurrentNodes[i];
                if (currentNode == null) {
                    continue;
                }

                StoryNode sn = currentNode.StoryNode;

                if (i > 500) {
                    Tell.Err("Limiting lane check, stopped after 500 lanes. Last node: " + currentNode);
                    break;
                }

                if (sn.Divider) {
                    var left = sn.LeftChild != null ? new StoryEventNode(sn.LeftChild?.Node, IntervalsPassed) : null;
                    var right = sn.RightChild != null ? new StoryEventNode(sn.RightChild?.Node, IntervalsPassed) : null;
                    HumanStoryteller.StoryComponent.CurrentNodes.Add(left);
                    HumanStoryteller.StoryComponent.CurrentNodes[i] = right;
                    _missedLastIncidentCheck = true;
                    yield return left;
                    _missedLastIncidentCheck = true;
                    _consecutiveEventCounter += 2; //Always execute a divider's children together
                    yield return right;
                } else {
                    if (laneCount > 400 && sn.LeftChild == null && sn.RightChild == null) {
                        HumanStoryteller.StoryComponent.CurrentNodes[i] = null;
                    } else {
                        StoryNode newEvent = HumanStoryteller.StoryComponent.Story.StoryGraph.TryNewEvent(currentNode, IntervalsPassed - currentNode.ExecuteTick);
                        if (newEvent == null) continue;
                        if (!newEvent.StoryEvent.Uuid.Equals(currentNode.StoryNode.StoryEvent.Uuid)) {
                            _missedLastIncidentCheck = true;
                            _consecutiveEventCounter++;
                        }

                        HumanStoryteller.StoryComponent.CurrentNodes[i] = new StoryEventNode(newEvent, IntervalsPassed);
                        yield return HumanStoryteller.StoryComponent.CurrentNodes[i];
                    }
                }
            }

            HumanStoryteller.StoryComponent.CurrentNodes.RemoveAll(item => item == null);
        }

        private void CheckStoryRefresh(object source, ElapsedEventArgs e) {
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || !(Find.TickManager.TicksGame > 0)) {
                RefreshTimer.Enabled = false;
                Tell.Warn("Tried to get story while not in-game");
                return;
            }

            Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId, story => HumanStoryteller.GetStoryCallback(story, this));
        }

        private void Init() {
            if (!HumanStoryteller.StoryComponent.Initialised) {
                HumanStoryteller.StoryComponent.Initialised = true;
                Tell.Log("STORYTELLER AWOKEN", HumanStoryteller.StoryComponent.StoryId);
                Current.Game.Scenario = Current.Game.Scenario.CopyForEditing();
                Tell.Log("SPLITTING UNIVERSE");
                HumanStoryteller.StoryComponent.FirstMapOfPlayer = Find.Maps.Find(x => x.ParentFaction.IsPlayer);
                HumanStoryteller.StoryComponent.SameAsLastEvent = HumanStoryteller.StoryComponent.FirstMapOfPlayer;
                Tell.Log("RECORDED HISTORY");
            } else {
                Tell.Log("CONTINUING HS GAME", HumanStoryteller.StoryComponent.StoryId);
            }

            HumanStoryteller.StoryComponent.ThreatCycle = this;

            Messages.Message(
                $"HumanStoryteller story: #{HumanStoryteller.StoryComponent.StoryId}, ALPHA BUILD: {HumanStoryteller.VERSION_NAME} ({HumanStoryteller.VERSION})",
                MessageTypeDefOf.PositiveEvent);

//            string str = "";
//            foreach (var def in from d in DefDatabase<MentalBreakDef>.AllDefs
//                select d) {
//                str += def.defName + "\n";
//            }
//
//            Tell.Log(str);
//            str = "";
//            foreach (var def in from d in DefDatabase<MentalBreakDef>.AllDefs
//                select d) {
//                str += def.label + "\n";
//            }
//            Tell.Log(str);

            if (HumanStoryteller.IsNoStory) {
                Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId, story => HumanStoryteller.GetStoryCallback(story, this));
            }

            RefreshTimer.Elapsed += CheckStoryRefresh;
            RefreshTimer.Interval = HumanStoryteller.LONG_REFRESH;
            RefreshTimer.Enabled = true;
        }

        public HumanStoryteller.RefreshRate CurrentRate => currentRate;

        public void SetRefreshRate(HumanStoryteller.RefreshRate rate) {
            switch (rate) {
                case HumanStoryteller.RefreshRate.Short:
                    RefreshTimer.Interval = HumanStoryteller.SHORT_REFRESH;
                    currentRate = HumanStoryteller.RefreshRate.Short;
                    break;
                case HumanStoryteller.RefreshRate.Medium:
                    RefreshTimer.Interval = HumanStoryteller.MEDIUM_REFRESH;
                    currentRate = HumanStoryteller.RefreshRate.Medium;
                    break;
                case HumanStoryteller.RefreshRate.Long:
                    RefreshTimer.Interval = HumanStoryteller.LONG_REFRESH;
                    currentRate = HumanStoryteller.RefreshRate.Long;
                    break;
                case HumanStoryteller.RefreshRate.Off:
                    RefreshTimer.Interval = HumanStoryteller.OFF_REFRESH;
                    currentRate = HumanStoryteller.RefreshRate.Off;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rate), rate, null);
            }
        }
    }
}