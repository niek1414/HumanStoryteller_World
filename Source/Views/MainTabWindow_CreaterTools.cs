using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Views {
    public class MainTabWindow_CreatorTools : MainTabWindow {
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;
        public override Vector2 RequestedTabSize => new Vector2(700, 420);

        private Vector2 _scrollPosition;
        private string _addLane;
        private string _lastMessage;
        private static Dictionary<string, Vector2> _scrollList = new Dictionary<string, Vector2>();

        public override void DoWindowContents(Rect inRect) {
            Text.Font = GameFont.Small;
            RenderDevWindow(inRect);
        }

        private void RenderDevWindow(Rect inRect) {
            RenderLaneInfo(new Rect(inRect.x, inRect.y, 400, inRect.height));
            RenderVariables(new Rect(inRect.x + 400, inRect.y + 25, inRect.width - 400, inRect.height));
        }

        private void RenderVariables(Rect inRect) {
            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);

            Widgets.Label(textRect, "Variables:");
            Rect varRect = new Rect(inRect.x, inRect.y + 25, inRect.width, 260);

            Rect viewRect = new Rect(0f, 0f, varRect.width - 16f, 35 * HumanStoryteller.StoryComponent.VariableBank.Count);
            Widgets.BeginScrollView(varRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);

            var keys = HumanStoryteller.StoryComponent.VariableBank.Keys.ToArray();
            for (var i = 0; i < keys.Length; i++) {
                if (!HumanStoryteller.StoryComponent.VariableBank.ContainsKey(keys[i])) {
                    continue;
                }

                Rect currentRect = listing.GetRect(25);
                Widgets.Label(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height), keys[i]);
                float temp = HumanStoryteller.StoryComponent.VariableBank[keys[i]];
                string buf = null;
                Widgets.TextFieldNumeric(new Rect(currentRect.x + currentRect.width / 2, currentRect.y, currentRect.width / 2, currentRect.height),
                    ref temp, ref buf, -9999999f);
                HumanStoryteller.StoryComponent.VariableBank[keys[i]] = temp;
            }

            listing.End();
            Widgets.EndScrollView();
            Rect zoneRect = new Rect(inRect.x, varRect.y + varRect.height, inRect.width, 50);

            if (Widgets.ButtonText(new Rect(zoneRect.x + zoneRect.width / 2, zoneRect.y + 20, zoneRect.width / 2, 30), "CopyHomeArea".Translate())) {
                Find.WindowStack.Add(new Window_CopyZone());
            }
        }

        private void RenderLaneInfo(Rect inRect) {
            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);
            Widgets.Label(textRect, "--DevMode window--");

            Rect textRect2 = new Rect(inRect.x, inRect.y + 25, inRect.width, 50);
            Widgets.Label(textRect2, "EventLanes".Translate() + ":");

            Rect laneRect = new Rect(inRect.x, inRect.y + 50, inRect.width, 250);
            Rect viewRect = new Rect(0f, 0f, laneRect.width - 16f, 60 * HumanStoryteller.StoryComponent.CurrentNodes.Count);
            Widgets.BeginScrollView(laneRect, ref _scrollPosition, viewRect);
            Listing listing = new Listing_Standard();
            listing.Begin(viewRect);
            for (int i = 0; i < HumanStoryteller.StoryComponent.CurrentNodes.Count; i++) {
                StoryEventNode currentNode = HumanStoryteller.StoryComponent.CurrentNodes[i];
                Rect currentRect = listing.GetRect(50);

                if (currentNode == null) {
                    Widgets.Label(new Rect(currentRect.x, currentRect.y, currentRect.width / 2, currentRect.height), "Pending".Translate() + "...");
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
                    "Destroy".Translate())) {
                    HumanStoryteller.StoryComponent.CurrentNodes[i] = null;
                }
            }

            listing.End();
            Widgets.EndScrollView();

            Rect addLaneRect = new Rect(inRect.x, laneRect.y + laneRect.height + 10, inRect.width, 50);
            Widgets.Label(new Rect(addLaneRect.x, addLaneRect.y, addLaneRect.width, 30), "AddLane".Translate());
            _addLane = Widgets.TextField(new Rect(addLaneRect.x, addLaneRect.y + 20, addLaneRect.width / 3 * 2, 30), _addLane);
            if (Widgets.ButtonText(new Rect(addLaneRect.x + addLaneRect.width / 3 * 2, addLaneRect.y + 20, addLaneRect.width / 3, 30), "Add".Translate())) {
                StoryNode sn = HumanStoryteller.StoryComponent.Story.StoryGraph.GetCurrentNode(_addLane);
                if (sn != null) {
                    HumanStoryteller.StoryComponent.CurrentNodes.Add(new StoryEventNode(sn, Find.TickManager.TicksGame / 600));
                    _lastMessage = "AddedNode".Translate();
                } else {
                    _lastMessage = "CouldNotFindNode".Translate();
                }
            }

            Widgets.Label(new Rect(addLaneRect.x, addLaneRect.y + 50, addLaneRect.width, 30), _lastMessage);
        }
    }
}