using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Patch {
    public class Main_Patch {
        public static void Patch(HarmonyInstance harmony) {
            MethodInfo storytellerTick = AccessTools.Method(typeof(Storyteller), "StorytellerTick");
            HarmonyMethod tick = new HarmonyMethod(typeof(StorytellerComp_HumanThreatCycle).GetMethod("Tick"));
            harmony.Patch(storytellerTick, tick);

            MethodInfo readoutOnGUI = AccessTools.Method(typeof(MouseoverReadout), "MouseoverReadoutOnGUI");
            HarmonyMethod onGUI = new HarmonyMethod(typeof(Main_Patch).GetMethod("OnGUI"));
            harmony.Patch(readoutOnGUI, null, onGUI);

            /** LOG TO FILE (if console is to small/limited)
            MethodBase log = AccessTools.Method(typeof(LogMessageQueue), "Enqueue");
            HarmonyMethod logConstr = new HarmonyMethod(typeof(Main_Patch).GetMethod("logConstr"));
            harmony.Patch(log, logConstr);
            */
        }

        public static void OnGUI() {
            if (HumanStoryteller.HumanStorytellerGame && HumanStoryteller.StoryComponent.Initialised) {
                HumanStoryteller.StoryComponent?.StoryOverlay?.DrawOverlay();
            }
            if (!HumanStoryteller.CreatorTools) return;
            if (UI.MouseCell().InBounds(Find.CurrentMap)) {
                Widgets.Label(new Rect(5, 5, 400, 30),
                    "tile:" + UI.MouseCell().x + ":" + UI.MouseCell().y + ":" + UI.MouseCell().z + " (storymaker info)");
            }
        }

        public static void logConstr(LogMessage msg) {
            FileLog.Log(msg.ToString());
        }
    }
}