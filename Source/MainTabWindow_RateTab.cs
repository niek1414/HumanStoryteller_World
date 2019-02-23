using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;
using static HumanStoryteller.StorytellerComp_HumanThreatCycle;

namespace HumanStoryteller {
    public class MainTabWindow_RateTab : MainTabWindow {
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;
        public override Vector2 RequestedTabSize => Prefs.DevMode ? new Vector2(700, 420) : new Vector2(596, 240);

        private Vector2 _scrollPosition;
        private int _rating = -1;
        private bool _refreshing;
        private string _addLane;
        private string _lastMessage;
        private static Dictionary<string, Vector2> _scrollList = new Dictionary<string, Vector2>();

        private static Dictionary<RefreshRate, string> _labels =
            new Dictionary<RefreshRate, string> {
                {RefreshRate.Short, "Short\n(2 min)"},
                {RefreshRate.Medium, "Medium\n(10 min)"},
                {RefreshRate.Long, "Long\n(1 hour)"},
                {RefreshRate.Off, "Off"}
            };

        public override void DoWindowContents(Rect inRect) {
            Text.Font = GameFont.Small;
            if (Prefs.DevMode) {
                RenderDevWindow(inRect);
            } else {
                RenderRating(inRect);
            }
        }

        private void RenderDevWindow(Rect inRect) {
            RenderLaneInfo(new Rect(inRect.x, inRect.y, 400, inRect.height));
            RenderVariables(new Rect(inRect.x + 400, inRect.y + 25, inRect.width - 400, inRect.height));
        }

        private void RenderVariables(Rect inRect) {
            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);

            Widgets.Label(textRect, "Variables:");
            Rect varRect = new Rect(inRect.x, inRect.y + 25, inRect.width, 300);
            Rect viewRect = new Rect(0f, 0f, varRect.width - 16f, 35 * StoryComponent.VariableBank.Count);
            Widgets.BeginScrollView(varRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);

            var keys = StoryComponent.VariableBank.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++) {
                if (!StoryComponent.VariableBank.ContainsKey(keys[i])) {
                    continue;
                }

                Rect currentRect = listing.GetRect(25);
                Widgets.Label(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height), keys[i]);
                float temp = StoryComponent.VariableBank[keys[i]];
                string buf = null;
                Widgets.TextFieldNumeric(new Rect(currentRect.x + currentRect.width / 2, currentRect.y, currentRect.width / 2, currentRect.height),
                    ref temp, ref buf, -9999999f);
                StoryComponent.VariableBank[keys[i]] = temp;
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private void RenderLaneInfo(Rect inRect) {
            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);
            Widgets.Label(textRect, "--DevMode window--");

            Rect textRect2 = new Rect(inRect.x, inRect.y + 25, inRect.width, 50);
            Widgets.Label(textRect2, "Event lanes:");

            Rect laneRect = new Rect(inRect.x, inRect.y + 50, inRect.width, 250);
            Rect viewRect = new Rect(0f, 0f, laneRect.width - 16f, 60 * StoryComponent.CurrentNodes.Count);
            Widgets.BeginScrollView(laneRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);
            for (int i = 0; i < StoryComponent.CurrentNodes.Count; i++) {
                StoryEventNode currentNode = StoryComponent.CurrentNodes[i];
                Rect currentRect = listing.GetRect(50);

                if (currentNode == null) {
                    Widgets.Label(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height),
                        "Pending...");
                    continue;
                }

                StoryNode sn = currentNode.StoryNode;

                if (!_scrollList.ContainsKey(sn.StoryEvent.Uuid)) {
                    _scrollList.Add(sn.StoryEvent.Uuid, new Vector2());
                }

                Vector2 scrollLocation = _scrollList[sn.StoryEvent.Uuid];
                Widgets.LabelScrollable(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height),
                    $"{sn.StoryEvent.Name}({sn.StoryEvent.Uuid})", ref scrollLocation);
                _scrollList[sn.StoryEvent.Uuid] = scrollLocation;

                if (Widgets.ButtonText(new Rect(currentRect.x + currentRect.width / 2 + 5, currentRect.y + 5, currentRect.width / 2 - 5, 40),
                    "destroy")) {
                    StoryComponent.CurrentNodes[i] = null;
                }
            }

            listing.End();
            Widgets.EndScrollView();

            Rect addLaneRect = new Rect(inRect.x, laneRect.y + laneRect.height + 10, inRect.width, 50);
            Widgets.Label(new Rect(addLaneRect.x, addLaneRect.y, addLaneRect.width, 30), "Add lane (enter uuid)");
            _addLane = Widgets.TextField(new Rect(addLaneRect.x, addLaneRect.y + 20, addLaneRect.width / 3 * 2, 30), _addLane);
            if (Widgets.ButtonText(new Rect(addLaneRect.x + addLaneRect.width / 3 * 2, addLaneRect.y + 20, addLaneRect.width / 3, 30), "Add")) {
                StoryNode sn = StoryComponent.Story.StoryGraph.GetCurrentNode(_addLane);
                if (sn != null) {
                    StoryComponent.CurrentNodes.Add(new StoryEventNode(sn));
                    _lastMessage = "Added!";
                } else {
                    _lastMessage = "Could not find node";
                }
            }

            Widgets.Label(new Rect(addLaneRect.x, addLaneRect.y + 50, addLaneRect.width, 30), _lastMessage);
        }

        private void RenderRating(Rect inRect) {
            if (_rating == -1 && !_refreshing) {
                _refreshing = true;
                RefreshRating();
            }

            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);
            if (IsNoStory) {
                Widgets.Label(textRect, "No story right now :(");
                return;
            }

            string ratingMessage = _rating == -1 ? "not voted" : "voted: " + (_rating + 1);
            Widgets.Label(textRect, $"Rate story:\n({ratingMessage})");

            for (int i = 0; i < 10; i++) {
                Rect buttonRect = new Rect(inRect.x + 30 + i * 50, inRect.y + 50, 50, 50);
                if (Widgets.ButtonText(buttonRect, (i + 1).ToString())) {
                    Storybook.SetRating(StoryId, i, RefreshRating);
                }
            }

            Rect switchLabelRect = new Rect(inRect.x, inRect.y + 110, inRect.width, 90);
            Widgets.Label(switchLabelRect, "Story sync rate with server (Pls use Short only when needed ;)\ne.g. when the story is created by a friend while you play)");
            var rates = (RefreshRate[]) Enum.GetValues(typeof(RefreshRate));
            for (var i = 0; i < rates.Length; i++) {
                Rect switchRect = new Rect(inRect.x + 50 + 130 * i, inRect.y + 150, 80, 50);
                if (Widgets.RadioButtonLabeled(switchRect, _labels[rates[i]], StoryComponent.ThreatCycle.CurrentRate == rates[i])) {
                    StoryComponent.ThreatCycle.SetRefreshRate(rates[i]);
                }
            }
        }

        private void RefreshRating() {
            Storybook.GetRating(StoryId, i => {
                _rating = i;
            });
        }
    }
}