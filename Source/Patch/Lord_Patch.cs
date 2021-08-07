using System.Reflection;
using HarmonyLib;
using HumanStoryteller.Helper.IntentHelper;
using Verse.AI.Group;

namespace HumanStoryteller.Patch {
    public class Lord_Patch {
        public static void Patch(Harmony harmony) {
            MethodInfo target = AccessTools.Method(typeof(Lord), "ReceiveMemo");
            HarmonyMethod pre = new HarmonyMethod(typeof(Lord_Patch).GetMethod("ReceiveMemo"));
            harmony.Patch(target, pre);
            
            MethodInfo exp = AccessTools.Method(typeof(Lord), "ExposeData");
            HarmonyMethod expPre = new HarmonyMethod(typeof(Lord_Patch).GetMethod("Expose"));
            harmony.Patch(exp, expPre);
        }

        public static void ReceiveMemo(Lord __instance, string memo) {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            if (__instance is LordWithMemory l) {
                if (memo.Equals("TravelArrived")) {
                    l.TraveledIR?.Traveled();
                }
            }
        }

        public static void Expose(Lord __instance) {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            if (__instance is LordWithMemory l) {
                l.ExposeData();
            }
        }
    }
}