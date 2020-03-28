using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Patch;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Web;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace HumanStoryteller {
    public class StorytellerCompProperties_HumanThreatCycle : StorytellerCompProperties {
        public StorytellerCompProperties_HumanThreatCycle() {
            compClass = typeof(StorytellerComp_HumanThreatCycle);
            Tell.Log("Init - version " + HumanStoryteller.VERSION + " " + HumanStoryteller.VERSION_NAME);
        }

        public static void StartHumanStorytellerGame(string storyId, string stageIdentifier, Story story = null) {
            try {
                List<Page> pageList = new List<Page> {
                    new Page_CreateWorldParams(),
                    new Page_SelectStartingSite(),
                    new Page_ConfigureStartingPawns {
                        nextAct = PageUtility.InitGameStart
                    }
                };

                Current.Game = new Game {InitData = new GameInitData(), Scenario = ScenarioDefOf.Crashlanded.scenario};
                Current.Game.Scenario.PreConfigure();
                Current.Game.storyteller = new Storyteller(DefDatabase<StorytellerDef>.GetNamed("Human"), DifficultyDefOf.Rough);
                HumanStoryteller.StoryComponent.StoryId = int.Parse(storyId);
                LongEventHandler.QueueLongEvent(
                    delegate {
                        HumanStoryteller.GetStoryCallback(story ?? Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId));
                        LongEventHandler.ExecuteWhenFinished(delegate { AfterStoryLoad(stageIdentifier, pageList); });
                    }, "LoadingStory",
                    true, StorytellerUI_Patch.ErrorWhileLoadingStory);
            } catch (Exception e) {
                Tell.Err(e.Message);
            }
        }

        private static void AfterStoryLoad(string stageIdentifier, List<Page> pageList) {
            if (stageIdentifier.Equals("W")) {
                Find.WindowStack.Add(PageUtility.StitchedPages(pageList));
                return;
            }

            pageList.RemoveAt(0);

            var planetCoverage = 0.05f;
            var seedString = GenText.RandomSeedString();
            var rainfall = OverallRainfall.Normal;
            var temperature = OverallTemperature.Normal;
            var initParams = HumanStoryteller.StoryComponent.Story.StoryGraph.InitParams();
            if (initParams != null && initParams.OverrideMapGen) {
                if (initParams.Seed != "") {
                    seedString = initParams.Seed;
                }

                if (initParams.Coverage.GetValue() != -1) {
                    planetCoverage = initParams.Coverage.GetValue();
                }

                if (initParams.Rainfall.GetValue() != -1) {
                    rainfall = CreateWorldUI_Patch.SeverityToRainfall(initParams.Rainfall.GetValue());
                }

                if (initParams.Temperature.GetValue() != -1) {
                    temperature = CreateWorldUI_Patch.SeverityToTemperature(initParams.Temperature.GetValue());
                }

                var value = initParams.PawnAmount.GetValue();
                if (value != -1) {
                    try {
                        if (Current.Game.Scenario.AllParts.First(o => typeof(ScenPart_ConfigPage_ConfigureStartingPawns) == o.GetType()) is
                            ScenPart_ConfigPage_ConfigureStartingPawns part) part.pawnCount = Mathf.RoundToInt(value);
                    } catch (InvalidOperationException) {
                    }
                }

                var open = initParams.Opening.GetWithoutInteractive();
                if (open != "") {
                    try {
                        if (Current.Game.Scenario.AllParts.First(o => typeof(ScenPart_GameStartDialog) == o.GetType()) is
                            ScenPart_GameStartDialog part) Traverse.Create(part).Field("text").SetValue(open);
                    } catch (InvalidOperationException) {
                    }
                }
            }

            LongEventHandler.QueueLongEvent(() => {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, rainfall, temperature);
                LongEventHandler.ExecuteWhenFinished(() => {
                    MemoryUtility.UnloadUnusedUnityAssets();
                    AfterWorldGeneration(stageIdentifier, initParams, pageList);
                });
            }, "GeneratingWorld", true, null);
        }

        private static void AfterWorldGeneration(string stageIdentifier, HumanIncidentParams_Root initParams, List<Page> pageList) {
            if (stageIdentifier.Equals("S")) {
                Find.WindowStack.Add(PageUtility.StitchedPages(pageList));
                Find.World.renderer.RegenerateAllLayersNow();
                return;
            }

            pageList.RemoveAt(0);

            if (initParams.OverrideMapLoc) {
                var site = Mathf.RoundToInt(initParams.Site.GetValue());
                if (site != -1) {
                    Find.GameInitData.startingTile = site;
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

            if (Find.GameInitData.startingTile == -1) {
                Find.GameInitData.ChooseRandomStartingTile();
            }

            if (stageIdentifier.Equals("C")) {
                Find.WindowStack.Add(PageUtility.StitchedPages(pageList));
                return;
            }

            PageUtility.InitGameStart();
        }
    }

    public class StorytellerDef_Human : StorytellerDef {
    }
}