using System.Reflection;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Patch {
    public class StoryStatus_Patch {
        public static void Patch(HarmonyInstance harmony) {
            //TODO fix on load when this is enabled
            // Slow motion
            MethodInfo speedMultiplier = AccessTools.Method(typeof(TickManager), "get_TickRateMultiplier");
            HarmonyMethod slowMotion = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("SlowMotion"));
            harmony.Patch(speedMultiplier, null, slowMotion);

            // Disable speed controls
            MethodInfo speedControl = AccessTools.Method(typeof(TimeControls), "DoTimeControlsGUI");
            HarmonyMethod speedButtons = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("SpeedButtons"));
            harmony.Patch(speedControl, speedButtons);

            // Disable camera jumping
            MethodInfo worldCameraJumper = AccessTools.Method(typeof(WorldCameraDriver), "JumpTo", new[] {typeof(int)});
            HarmonyMethod enableCameraControls = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("EnableCameraControls"));
            harmony.Patch(worldCameraJumper, enableCameraControls);
            MethodInfo driverCameraJumper = AccessTools.Method(typeof(CameraDriver), "JumpToCurrentMapLoc", new[] {typeof(Vector3)});
            harmony.Patch(driverCameraJumper, enableCameraControls);

            // Disable camera panning
            MethodInfo preventProp = AccessTools.Method(typeof(CameraDriver), "get_AnythingPreventsCameraMotion");
            HarmonyMethod cameraDollyPrevention = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("CameraDollyPrevention"));
            harmony.Patch(preventProp, null, cameraDollyPrevention);
            MethodInfo isInputBlockedNow = AccessTools.Method(typeof(Mouse), "get_IsInputBlockedNow");
            harmony.Patch(isInputBlockedNow, null, cameraDollyPrevention);

            // Disable all user events
            MethodInfo highPrio = AccessTools.Method(typeof(WindowStack), "HandleEventsHighPriority");
            HarmonyMethod disableUserInput = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("DisableUserInput"));
            harmony.Patch(highPrio, null, disableUserInput);

            // Disable all UI clutter
            MethodInfo mapUI = AccessTools.Method(typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs");
            HarmonyMethod removeUI = new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("RemoveUI"));
            harmony.Patch(mapUI, removeUI);
            MethodInfo cellInfoUI = AccessTools.Method(typeof(MouseoverReadout), "MouseoverReadoutOnGUI");
            harmony.Patch(cellInfoUI, removeUI);
            MethodInfo alertUI = AccessTools.Method(typeof(AlertsReadout), "AlertsReadoutOnGUI");
            harmony.Patch(alertUI, removeUI);
            MethodInfo tutorUI = AccessTools.Method(typeof(Tutor), "TutorOnGUI");
            harmony.Patch(tutorUI, removeUI);
            MethodInfo windowsUI = AccessTools.Method(typeof(WindowStack), "WindowStackOnGUI");
            HarmonyMethod removeUIAndShowForcedPauseWindows =
                new HarmonyMethod(typeof(StoryStatus_Patch).GetMethod("RemoveUIAndShowForcedPauseWindows"));
            harmony.Patch(windowsUI, removeUIAndShowForcedPauseWindows);
        }

        public static void SlowMotion(TickManager __instance, ref float __result) {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return;
            var sc = HumanStoryteller.StoryComponent;
            if (sc.StoryStatus.ForcedSlowMotion && !__instance.Paused) {
                __result = 0.3f;
            }
        }

        public static bool SpeedButtons() {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return true;
            var sc = HumanStoryteller.StoryComponent;
            if (sc.StoryStatus.DisableSpeedControls && Current.Game.tickManager.CurTimeSpeed == TimeSpeed.Paused) {
                Current.Game.tickManager.CurTimeSpeed = TimeSpeed.Normal;
            }

            return !sc.StoryStatus.DisableSpeedControls;
        }

        public static bool RemoveUIAndShowForcedPauseWindows(WindowStack __instance) {
            var shouldNotRemoveUI = RemoveUI();
            if (!shouldNotRemoveUI) {
                if (Current.Game.tickManager.CurTimeSpeed == TimeSpeed.Paused) {
                    Current.Game.tickManager.CurTimeSpeed = TimeSpeed.Normal;
                }

                foreach (var w in __instance.Windows) {
                    if (w.forcePause || w.GetType() == typeof(EditWindow_Log)) {
                        w.ExtraOnGUI();
                        if (w.drawShadow) {
                            GUI.color = new Color(1f, 1f, 1f, w.shadowAlpha);
                            Widgets.DrawShadowAround(w.windowRect);
                            GUI.color = Color.white;
                        }

                        w.WindowOnGUI();
                    }
                }
            }

            return shouldNotRemoveUI;
        }

        public static bool RemoveUI() {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return true;
            var sc = HumanStoryteller.StoryComponent;
            return !(sc.StoryStatus.MovieMode || sc.StoryOverlay.ShouldBlockInput());
        }

        public static void CameraDollyPrevention(ref bool __result) {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return;
            var sc = HumanStoryteller.StoryComponent;
            if (sc.StoryStatus.DisableCameraControls || sc.StoryStatus.MovieMode || sc.StoryOverlay.ShouldBlockInput()) {
                __result = true;
            }
        }

        public static void DisableUserInput() {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return;
            if (!(HumanStoryteller.StoryComponent.StoryStatus.MovieMode || HumanStoryteller.StoryComponent.StoryOverlay.ShouldBlockInput())) return;
            if (KeyBindingDefOf.TogglePause.KeyDownEvent) {
                Find.MainTabsRoot.ToggleTab(MainButtonDefOf.Menu);
            }

            if (Event.current != null
                && Event.current.type != EventType.layout
                && Event.current.type != EventType.repaint
                && Event.current.type != EventType.ignore
                && !KeyBindingDefOf.Dev_ToggleDebugLog.KeyDownEvent) {
                Event.current.Use();
            }
        }

        public static bool EnableCameraControls() {
            if (Main_Patch.ShouldNotMessWithGame() || LongEventHandler.ForcePause) return true;
            var sc = HumanStoryteller.StoryComponent;
            if (sc.StoryStatus.JumpException) return true;
            return !(sc.StoryStatus.DisableCameraControls || sc.StoryStatus.MovieMode || sc.StoryOverlay.ShouldBlockInput());
        }
    }
}