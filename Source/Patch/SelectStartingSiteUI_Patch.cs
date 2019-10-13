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
            if (HumanStoryteller.CreatorTools) {
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(5, 5, 400, 30), "tile:" + Find.WorldInterface.SelectedTile + " (storymaker info)");
            }

            if (HumanStoryteller.IsNoStory)
                return;

            var initParams = HumanStoryteller.StoryComponent.Story.StoryGraph.InitParams();
            if (initParams == null) return;

            if (!initParams.OverrideMapLoc)
                return;
            var site = Mathf.RoundToInt(initParams.Site.GetValue());
            if (site != -1) {
                if (Find.GameInitData.startingTile != site) {
                    Find.GameInitData.startingTile = site;
                    Find.WorldInterface.SelectedTile = site;
                    Find.WorldCameraDriver.JumpTo(Find.WorldGrid.GetTileCenter(site));
                }

                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(UI.screenWidth / 2 - 300, UI.screenHeight - 100, 1000, 45), "LocationOverriden".Translate());
                Text.Font = GameFont.Small;
            }

            if (initParams.StartSeason != "") {
                switch (initParams.StartSeason) {
                    case "Auto":
                        Find.GameInitData.startingSeason = Season.Undefined;
                        break;
                    case "Spring":
                        Find.GameInitData.startingSeason = Season.Spring;
                        break;
                    case "Summer":
                        Find.GameInitData.startingSeason = Season.Summer;
                        break;
                    case "Fall":
                        Find.GameInitData.startingSeason = Season.Fall;
                        break;
                    case "Winter":
                        Find.GameInitData.startingSeason = Season.Winter;
                        break;
                    case "PermanentSummer":
                        Find.GameInitData.startingSeason = Season.PermanentSummer;
                        break;
                    case "PermanentWinter":
                        Find.GameInitData.startingSeason = Season.PermanentWinter;
                        break;
                }
            }

            var mapSize = Mathf.RoundToInt(initParams.MapSize.GetValue());
            if (mapSize != -1) {
                Find.GameInitData.mapSize = mapSize;
            }
        }
    }
}