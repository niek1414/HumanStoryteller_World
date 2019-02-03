using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller {
    public class MainTabWindow_RateTab : MainTabWindow {
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;
        public override Vector2 RequestedTabSize => new Vector2(536, 150);

        private int _rating = -1;
        private bool _refreshing;

        public override void DoWindowContents(Rect inRect) {
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