using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Patch {
    public class CreateWorldUI_Patch {
        public static void Patch(Harmony harmony) {
            MethodInfo targetMain = AccessTools.Method(typeof(Page_CreateWorldParams), "DoWindowContents");

            HarmonyMethod draw = new HarmonyMethod(typeof(CreateWorldUI_Patch).GetMethod("DoWindowContents"));

            harmony.Patch(targetMain, null, draw);
        }

        public static void DoWindowContents(Rect rect) {
            if (HumanStoryteller.IsNoStory) return;
            if (Find.WindowStack == null) return;
            if (Find.WindowStack.currentlyDrawnWindow is Page_CreateWorldParams page) {
                var initParams = HumanStoryteller.StoryComponent.Story.StoryGraph.InitParams();
                if (initParams == null) return;

                if (!initParams.OverrideMapGen)
                    return;
                if (initParams.Seed != "") {
                    Traverse.Create(page).Field("seedString").SetValue(initParams.Seed);
                }

                if (initParams.Coverage.GetValue() != -1) {
                    Traverse.Create(page).Field("planetCoverage").SetValue(initParams.Coverage.GetValue());
                }

                if (initParams.Rainfall.GetValue() != -1) {
                    Traverse.Create(page).Field("rainfall").SetValue(SeverityToRainfall(initParams.Rainfall.GetValue()));
                }

                if (initParams.Temperature.GetValue() != -1) {
                    Traverse.Create(page).Field("temperature").SetValue(SeverityToTemperature(initParams.Temperature.GetValue()));
                }

                if (initParams.Population.GetValue() != -1) {
                    Traverse.Create(page).Field("population").SetValue(SeverityToPopulation(initParams.Population.GetValue()));
                }

                Widgets.Label(new Rect(rect.x, rect.y + 280, rect.width, 30), "ParametersOverriden".Translate());

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
        }

        public static OverallRainfall SeverityToRainfall(float severity) {
            if (severity == -1) {
                return OverallRainfall.Normal;
            }

            if (severity < 0.14f) {
                return OverallRainfall.AlmostNone;
            }

            if (severity < 0.28f) {
                return OverallRainfall.Little;
            }

            if (severity < 0.43f) {
                return OverallRainfall.LittleBitLess;
            }

            if (severity < 0.58f) {
                return OverallRainfall.Normal;
            }

            if (severity < 0.72f) {
                return OverallRainfall.LittleBitMore;
            }

            if (severity < 0.87f) {
                return OverallRainfall.High;
            }

            return OverallRainfall.VeryHigh;
        }

        public static OverallTemperature SeverityToTemperature(float severity) {
            if (severity == -1) {
                return OverallTemperature.Normal;
            }

            if (severity < 0.14f) {
                return OverallTemperature.VeryCold;
            }

            if (severity < 0.28f) {
                return OverallTemperature.Cold;
            }

            if (severity < 0.43f) {
                return OverallTemperature.LittleBitColder;
            }

            if (severity < 0.58f) {
                return OverallTemperature.Normal;
            }

            if (severity < 0.72f) {
                return OverallTemperature.LittleBitWarmer;
            }

            if (severity < 0.87f) {
                return OverallTemperature.Hot;
            }

            return OverallTemperature.VeryHot;
        }

        public static OverallPopulation SeverityToPopulation(float severity) {
            if (severity == -1) {
                return OverallPopulation.Normal;
            }

            if (severity < 0.2f) {
                return OverallPopulation.Little;
            }

            if (severity < 0.4f) {
                return OverallPopulation.LittleBitLess;
            }

            if (severity < 0.6f) {
                return OverallPopulation.Normal;
            }

            if (severity < 0.8f) {
                return OverallPopulation.LittleBitMore;
            }

            if (severity < 1f) {
                return OverallPopulation.High;
            }

            return OverallPopulation.VeryHigh;
        }
    }
}