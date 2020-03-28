using System;
using System.Reflection;
using Harmony;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Patch {
    public class Main_Patch {
        public static void Patch(HarmonyInstance harmony) {
            MethodInfo quickstart = AccessTools.Method(typeof(QuickStarter), "CheckQuickStart");
            HarmonyMethod checkQuickstart = new HarmonyMethod(typeof(Main_Patch).GetMethod("CheckQuickStart"));
            harmony.Patch(quickstart, checkQuickstart);

            MethodInfo storytellerTick = AccessTools.Method(typeof(Storyteller), "StorytellerTick");
            HarmonyMethod tick = new HarmonyMethod(typeof(StorytellerComp_HumanThreatCycle).GetMethod("Tick"));
            harmony.Patch(storytellerTick, tick);

            MethodInfo readoutOnGUI = AccessTools.Method(typeof(MouseoverReadout), "MouseoverReadoutOnGUI");
            HarmonyMethod onGUI = new HarmonyMethod(typeof(Main_Patch).GetMethod("OnGUI"));
            harmony.Patch(readoutOnGUI, null, onGUI);

            MethodInfo readoutOnUpdate = AccessTools.Method(typeof(TargetHighlighter), "TargetHighlighterUpdate");
            HarmonyMethod onUpdate = new HarmonyMethod(typeof(Main_Patch).GetMethod("OnUpdate"));
            harmony.Patch(readoutOnUpdate, onUpdate);

            MethodInfo checkOrUpdateGameOver = AccessTools.Method(typeof(GameEnder), "CheckOrUpdateGameOver");
            HarmonyMethod onPossibleGameOver = new HarmonyMethod(typeof(Main_Patch).GetMethod("OnPossibleGameOver"));
            harmony.Patch(checkOrUpdateGameOver, onPossibleGameOver);

            MethodInfo canNameAnythingNow = AccessTools.Method(typeof(NamePlayerFactionAndSettlementUtility), "CanNameAnythingNow");
            HarmonyMethod onTryNameAnything = new HarmonyMethod(typeof(Main_Patch).GetMethod("OnTryNameAnything"));
            harmony.Patch(canNameAnythingNow, onTryNameAnything);

            MethodInfo stripTags = AccessTools.Method(typeof(ColoredText), "StripTags");
            HarmonyMethod stripTagMethod = new HarmonyMethod(typeof(Main_Patch).GetMethod("StripTags"));
            harmony.Patch(stripTags, stripTagMethod);

            /** LOG TO FILE (if console is to small/limited)
            MethodBase log = AccessTools.Method(typeof(LogMessageQueue), "Enqueue");
            HarmonyMethod logConstr = new HarmonyMethod(typeof(Main_Patch).GetMethod("logConstr"));
            harmony.Patch(log, logConstr);*/
        }

        public static void CheckQuickStart() {
            if (HumanStoryteller.DidInitialParamCheck) return;
            HumanStoryteller.DidInitialParamCheck = true;
            foreach (var arg in Environment.GetCommandLineArgs()) {
                if (arg.StartsWith("HumanStoryteller")) {
                    var split = arg.Split(':');
                    StorytellerCompProperties_HumanThreatCycle.StartHumanStorytellerGame(split[1], split[2]);
                }
            }

            HumanStoryteller.CheckDebugConnectionSetting();
        }

        public static bool ShouldNotMessWithGame() {
            if (Current.Game == null) return true;
            var sc = HumanStoryteller.StoryComponent;
            return HumanStoryteller.IsNoStory || sc == null || !sc.Initialised;
        }

        public static void DebugFunction(Thing __instance) {
            Tell.Debug("map count: " + Find.Maps.Count);
            Tell.Debug("mapIndexOrState: " + Traverse.Create(__instance).Field("mapIndexOrState").GetValue());
        }

        public static void OnGUI() {
            try {
                if (HumanStoryteller.HumanStorytellerGame && HumanStoryteller.StoryComponent.Initialised) {
                    HumanStoryteller.StoryComponent?.StoryOverlay?.DrawOverlay();
                }

                if (!HumanStoryteller.CreatorTools) return;
                if (UI.MouseCell().InBounds(Find.CurrentMap)) {
                    Text.Font = GameFont.Small;
                    Widgets.Label(new Rect(5, 5, 400, 30),
                        "tile:" + UI.MouseCell().x + ":" + UI.MouseCell().y + ":" + UI.MouseCell().z + " (storymaker info)");
                }
            } catch (Exception e) {
                Tell.Err("Error while drawing overlay, " + e.Message, e);
            }
        }

        public static void OnUpdate() {
            if (HumanStoryteller.HumanStorytellerGame && HumanStoryteller.StoryComponent.Initialised) {
                HumanStoryteller.StoryComponent?.StoryOverlay?.DrawHighPrio();
            }
        }

        public static bool OnPossibleGameOver() {
            if (ShouldNotMessWithGame()) return true;

            return !HumanStoryteller.StoryComponent.StoryStatus.DisableGameOverDialog;
        }

        public static bool OnTryNameAnything() {
            if (ShouldNotMessWithGame()) return true;

            return !HumanStoryteller.StoryComponent.StoryStatus.DisableNameColonyDialog;
        }

        public static Regex Tag = new Regex("<(?!.*size)[^>]*>");

        public static bool StripTags(ref string __result, string s) {
            if (s.NullOrEmpty() || s.IndexOf('<') < 0) {
                __result = s;
                return false;
            }

            __result = Tag.Replace(s, string.Empty);
            return false;
        }

        public static void logConstr(LogMessage msg) {
            FileLog.Log(msg.ToString());
        }
    }
}