using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller {
    public class MainTabWindow_RateTab : MainTabWindow {
        public static int inputValue = 0;

        public static string inputBuffer = "00";

        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;

        public override Vector2 RequestedTabSize => new Vector2(450, 355);

        public override void DoWindowContents(Rect inRect) {
            Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);
            if (StorytellerComp_HumanThreatCycle.IsNoStory) {
                Widgets.TextArea(textRect, "No story right now :( ", true);
                return;
            }
            Widgets.TextArea(textRect, "Rate story: ", true);

            for (int i = 0; i < 10; i++) {
                Rect buttonRect = new Rect(inRect.x + i * 50, inRect.y + 60, 50, 50);
                if (Widgets.ButtonText(buttonRect, i.ToString())) {
                    Storybook.SetRating(StorytellerComp_HumanThreatCycle.StoryId, i, () => {});
                }
            }
        }
    }
}