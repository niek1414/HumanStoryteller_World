using System.Reflection;
using Harmony;
using RimWorld;

namespace HumanStoryteller.Patch {
    public class Main_Patch {
        public static void Patch(HarmonyInstance harmony) {
            MethodInfo target = AccessTools.Method(typeof(Storyteller), "StorytellerTick");

            HarmonyMethod tick = new HarmonyMethod(typeof(StorytellerComp_HumanThreatCycle).GetMethod("Tick"));

            harmony.Patch(target, tick);
        }
    }
}