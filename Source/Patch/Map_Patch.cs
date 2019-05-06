using System.Reflection;
using Harmony;
using HumanStoryteller.Util;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Patch {
    public class Map_Patch {
        public static void Patch(HarmonyInstance harmony) {
            MethodInfo targetEmpty = AccessTools.Method(typeof(SettleInEmptyTileUtility), "Settle");
            MethodInfo targetExisting = AccessTools.Method(typeof(SettleInExistingMapUtility), "Settle");
            HarmonyMethod pre = new HarmonyMethod(typeof(Map_Patch).GetMethod("Settle"));
            harmony.Patch(targetEmpty, pre);
            harmony.Patch(targetExisting, pre);
        }

        public static void Settle(){
            HumanStoryteller.StoryComponent.LastColonizedMap = Find.CurrentMap;
        }
    }
}