using System.Reflection;
using Harmony;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Patch {
    public class SelectStartingSiteUI_Patch {
        public static void Patch(HarmonyInstance harmony) {
            MethodInfo targetMain = AccessTools.Method(typeof(Page_SelectStartingSite), "ExtraOnGUI");

            HarmonyMethod draw = new HarmonyMethod(typeof(SelectStartingSiteUI_Patch).GetMethod("ExtraOnGUI"));

            harmony.Patch(targetMain, null, draw);
        }

        public static void ExtraOnGUI() {
            if (Prefs.DevMode) {
                Widgets.Label(new Rect(5, 5, 400, 30), "tile:" + Find.WorldInterface.SelectedTile + " (storymaker info)");
            }

            if (HumanStoryteller.IsNoStory)
                return;

            var initParams = HumanStoryteller.StoryComponent.Story.StoryGraph.InitParams();
            if (initParams == null) return;

            if (!initParams.OverrideMapLoc)
                return;
            var site = Mathf.RoundToInt(initParams.Site.GetValue());
            if (Find.GameInitData.startingTile != site) {
                Find.GameInitData.startingTile = site;
                Find.WorldInterface.SelectedTile = site;
                Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(site));
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(UI.screenWidth / 2 - 300, UI.screenHeight - 100, 1000, 45), "LocationOverriden".Translate());
            Text.Font = GameFont.Small;
        }
    }
}