using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Util {
    public class MapUtil {
        private static int _cleanupCounter;
        private const int CleanupCounterMax = 10;

        public static Map GetMapByName(String name, bool warn = true) {
            var mapBank = StorytellerComp_HumanThreatCycle.StoryComponent.MapBank;

            _cleanupCounter++;
            if (_cleanupCounter >= CleanupCounterMax) {
                _cleanupCounter = 0;
                foreach (var item in mapBank.Where(pair =>
                    pair.Value == null
                    || pair.Value.Tile == -1).ToList()) {
                    mapBank.Remove(item.Key);
                }
            }

            foreach (var pair in mapBank) {
                if (pair.Key.ToUpper().Equals(name.ToUpper())) {
                    if (pair.Value == null) {
                        if (warn)
                            Tell.Warn("Requested map does not exist (anymore)", name);
                        return null;
                    }

                    var map = pair.Value.Map;
                    if (map == null && warn) {
                        Tell.Warn("Requested map is not created yet (check first if map is created)", name);
                    }

                    return map;
                }
            }

            if (warn)
                Tell.Warn("Requested map does not exist (anymore)", name);
            return null;
        }

        public static Map FirstOfPlayer() {
            return StorytellerComp_HumanThreatCycle.StoryComponent.FirstMapOfPlayer;
        }

        public static Map SameAsLastEvent() {
            return StorytellerComp_HumanThreatCycle.StoryComponent.SameAsLastEvent;
        }

        public static void SaveMapByName(String name, MapParent map) {
            if (StorytellerComp_HumanThreatCycle.StoryComponent.PawnBank.ContainsKey(name)) {
                RemoveName(name);
            }

            StorytellerComp_HumanThreatCycle.StoryComponent.MapBank.Add(name, map);
        }

        public static void RemoveName(string name) {
            StorytellerComp_HumanThreatCycle.StoryComponent.MapBank.Remove(name);
        }

        public static bool MapExists(Map map) {
            var mapBank = StorytellerComp_HumanThreatCycle.StoryComponent.MapBank;
            foreach (var pair in mapBank) {
                if (pair.Value?.Map == map) {
                    return true;
                }
            }

            return false;
        }
    }
}