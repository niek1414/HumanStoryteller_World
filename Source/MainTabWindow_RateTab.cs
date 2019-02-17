using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller {
    public class MainTabWindow_RateTab : MainTabWindow {
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;
        public override Vector2 RequestedTabSize => Prefs.DevMode ? new Vector2(700, 420) : new Vector2(536, 150);

        private Vector2 _scrollPosition;
        private int _rating = -1;
        private bool _refreshing;
        private string _addLane;
        private string _lastMessage;
        private static Dictionary<string, Vector2> _scrollList = new Dictionary<string, Vector2>();
        private static Dictionary<string, string> _fieldBuf = new Dictionary<string, string>();

        public override void DoWindowContents(Rect inRect) {
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
            Rect viewRect = new Rect(0f, 0f, varRect.width - 16f, 35 * StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank.Count);
            Widgets.BeginScrollView(varRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);

            var keys = StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++) {
                if (!StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank.ContainsKey(keys[i])) {
                    continue;
                }
                Rect currentRect = listing.GetRect(25);
                Widgets.Label(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height), keys[i]);
                float temp = StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank[keys[i]];
                string buf = null;
                Widgets.TextFieldNumeric(new Rect(currentRect.x + currentRect.width / 2, currentRect.y, currentRect.width / 2, currentRect.height), ref temp, ref buf, -9999999f);
                StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank[keys[i]] = temp;
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
            Rect viewRect = new Rect(0f, 0f, laneRect.width - 16f, 60 * StorytellerComp_HumanThreatCycle.StoryComponent.CurrentNodes.Count);
            Widgets.BeginScrollView(laneRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);
            for (int i = 0; i < StorytellerComp_HumanThreatCycle.StoryComponent.CurrentNodes.Count; i++) {
                StoryEventNode currentNode = StorytellerComp_HumanThreatCycle.StoryComponent.CurrentNodes[i];
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
                    StorytellerComp_HumanThreatCycle.StoryComponent.CurrentNodes[i] = null;
                }
            }

            listing.End();
            Widgets.EndScrollView();

            Rect addLaneRect = new Rect(inRect.x, laneRect.y + laneRect.height + 10, inRect.width, 50);
            Widgets.Label(new Rect(addLaneRect.x, addLaneRect.y, addLaneRect.width, 30), "Add lane (enter uuid)");
            _addLane = Widgets.TextField(new Rect(addLaneRect.x, addLaneRect.y + 20, addLaneRect.width / 3 * 2, 30), _addLane);
            if (Widgets.ButtonText(new Rect(addLaneRect.x + addLaneRect.width / 3 * 2, addLaneRect.y + 20, addLaneRect.width / 3, 30), "Add")) {
                StoryNode sn = StorytellerComp_HumanThreatCycle.StoryComponent.Story.StoryGraph.GetCurrentNode(_addLane);
                if (sn != null) {
                    StorytellerComp_HumanThreatCycle.StoryComponent.CurrentNodes.Add(new StoryEventNode(sn));
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
            if (StorytellerComp_HumanThreatCycle.IsNoStory) {
                Widgets.TextArea(textRect, "No story right now :(", true);
                return;
            }

            string ratingMessage = _rating == -1 ? "not voted" : "voted: " + (_rating + 1);
            Widgets.TextArea(textRect, $"Rate story:\n({ratingMessage})", true);

            for (int i = 0; i < 10; i++) {
                Rect buttonRect = new Rect(inRect.x + i * 50, inRect.y + 60, 50, 50);
                if (Widgets.ButtonText(buttonRect, (i + 1).ToString())) {
                    Storybook.SetRating(StorytellerComp_HumanThreatCycle.StoryId, i, RefreshRating);
                }
            }
        }

        private void RefreshRating() {
            Storybook.GetRating(StorytellerComp_HumanThreatCycle.StoryId, i => {
                _rating = i;
                _refreshing = false;
            });
        }
    }
}