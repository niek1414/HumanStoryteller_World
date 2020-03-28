using System;
using System.Reflection;
using HarmonyLib;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Patch {
    public class Map_Patch {
        public static void Patch(Harmony harmony) {
            MethodInfo targetEmpty = AccessTools.Method(typeof(SettleInEmptyTileUtility), "Settle");
            MethodInfo targetExisting = AccessTools.Method(typeof(SettleInExistingMapUtility), "Settle");
            HarmonyMethod patchSettle = new HarmonyMethod(typeof(Map_Patch).GetMethod("Settle"));
            harmony.Patch(targetEmpty, null, patchSettle);
            harmony.Patch(targetExisting, null, patchSettle);

            MethodInfo targetEnter = AccessTools.Method(typeof(CaravanEnterMapUtility), "Enter",
                new[] {typeof(Caravan), typeof(Map), typeof(Func<Pawn, IntVec3>), typeof(CaravanDropInventoryMode), typeof(bool)});
            HarmonyMethod patchEnter = new HarmonyMethod(typeof(Map_Patch).GetMethod("Enter"));
            harmony.Patch(targetEnter, null, patchEnter);

            MethodInfo targetStopped = AccessTools.Method(typeof(Caravan_PathFollower), "PatherArrived");
            HarmonyMethod patchArrive = new HarmonyMethod(typeof(Map_Patch).GetMethod("Arrive"));
            harmony.Patch(targetStopped, patchArrive);

            MethodInfo targetPostLoad = AccessTools.Method(typeof(PostLoadIniter), "DoAllPostLoadInits");
            HarmonyMethod patchPostLoad = new HarmonyMethod(typeof(Map_Patch).GetMethod("PostLoad"));
            harmony.Patch(targetPostLoad, null, patchPostLoad);

            MethodInfo targetRegenerateSection = AccessTools.Method(typeof(Section), "RegenerateAllLayers");
            HarmonyMethod patchIfMapExists = new HarmonyMethod(typeof(Map_Patch).GetMethod("ConnectIfGenerateLayer"));
            HarmonyMethod patchDisconnectMap = new HarmonyMethod(typeof(Map_Patch).GetMethod("DisconnectMapAfterGenerateLayer"));
            harmony.Patch(targetRegenerateSection, patchIfMapExists, patchDisconnectMap);

            MethodInfo shouldRemoveMapNowSettlementBase = AccessTools.Method(typeof(Settlement), "ShouldRemoveMapNow");
            MethodInfo shouldRemoveMapNowSite = AccessTools.Method(typeof(Site), "ShouldRemoveMapNow");
            MethodInfo shouldRemoveMapNowCaravansBattlefield = AccessTools.Method(typeof(CaravansBattlefield), "ShouldRemoveMapNow");
            MethodInfo shouldRemoveMapNowDestroyedSettlement = AccessTools.Method(typeof(DestroyedSettlement), "ShouldRemoveMapNow");
            HarmonyMethod shouldRemoveMapNowPatch = new HarmonyMethod(typeof(Map_Patch).GetMethod("ShouldRemoveMapNow"));
            harmony.Patch(shouldRemoveMapNowSettlementBase, shouldRemoveMapNowPatch);
            harmony.Patch(shouldRemoveMapNowSite, shouldRemoveMapNowPatch);
            harmony.Patch(shouldRemoveMapNowCaravansBattlefield, shouldRemoveMapNowPatch);
            harmony.Patch(shouldRemoveMapNowDestroyedSettlement, shouldRemoveMapNowPatch);
        }

        public static void Settle() {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            var map = Find.CurrentMap;
            HumanStoryteller.StoryComponent.LastColonizedMap = map;
            Enter(map);
        }

        public static void Enter(Map map) {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            FloodFillerFog.DebugRefogMap(map);
        }

        public static bool ShouldRemoveMapNow(MapParent __instance, ref bool __result, out bool alsoRemoveWorldObject) {
            alsoRemoveWorldObject = false;
            if (Main_Patch.ShouldNotMessWithGame() || !__instance.HasMap) return true;

            if (MapUtil.CheckIfMapIsPersistent(__instance.Map)) {
                __result = false;
                return false;
            }

            return true;
        }

        public static void Arrive(Caravan __instance) {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            var container = MapUtil.GetMapContainerByTile(__instance.Tile, false);
            if (container == null) {
                return;
            }

            if (container.IsDecoupled) {
                Tell.Log("Found decoupled map on tile " + __instance.Tile + ". Trying to couple..");
                container.TryCouple();
            }
        }

        public static void PostLoad() {
            if (Main_Patch.ShouldNotMessWithGame()) return;
            try {
                foreach (var container in HumanStoryteller.StoryComponent.MapBank) {
                    container.Value.ExposeDataAfter();
                }
            } catch (NullReferenceException) {
                Tell.Log("Nullpointer on map load right after save, has no impact on gameplay (but really should be fixed in the future...)");
            }
        }

        public static void ConnectIfGenerateLayer(Section __instance, out bool __state) {
            __state = false;
            if (__instance.map != null && Find.Maps.Contains(__instance.map)) return;
            if (MapUtil.MapExists(__instance.map)) {
                MapUtil.GetMapContainerByTile(__instance.map.Tile).FakeConnect();
                __state = true;
            } else {
                Tell.Warn("Generating layer for unknown map. Is map null? : " + (__instance.map == null), __instance.map.ToStringSafe());
            }
        }

        public static void DisconnectMapAfterGenerateLayer(Section __instance, bool __state) {
            if (__state) {
                MapUtil.GetMapContainerByTile(__instance.map.Tile).FakeDisconnect();
            }
        }
    }
}