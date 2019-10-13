using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util.Logging;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Util {
    public class MapUtil {
        private static int _cleanupCounter;
        private const int CleanupCounterMax = 10;

        public static Map GetMapByName(String name, bool warn = true) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;

            _cleanupCounter++;
            if (_cleanupCounter >= CleanupCounterMax) {
                _cleanupCounter = 0;
                foreach (var item in mapBank.Where(pair => pair.Value == null).ToList()) {
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

                    var map = pair.Value.GetMap();
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

        public static Map GetMapByTile(long tile, bool warn = true) {
            return GetMapContainerByTile(tile, warn).GetMap();
        }

        public static MapContainer GetMapContainerByTile(long tile, bool warn = true) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;

            _cleanupCounter++;
            if (_cleanupCounter >= CleanupCounterMax) {
                _cleanupCounter = 0;
                foreach (var item in mapBank.Where(pair =>
                    pair.Value == null
                    || pair.Value.Parent.Tile == -1).ToList()) {
                    mapBank.Remove(item.Key);
                }
            }

            foreach (var pair in mapBank) {
                if (pair.Value?.Parent.Tile == tile) {
                    if (pair.Value.IsDecoupled) {
                        return pair.Value;
                    }

                    var parent = pair.Value.Parent;
                    if (parent == null && warn) {
                        Tell.Warn("Requested map is not created yet (check first if map is created)", tile);
                    }

                    return pair.Value;
                }
            }

            if (warn)
                Tell.Warn("Requested map does not exist (anymore)", tile);
            return null;
        }

        public static Map FirstOfPlayer() {
            return HumanStoryteller.StoryComponent.FirstMapOfPlayer;
        }

        public static Map SameAsLastEvent() {
            return HumanStoryteller.StoryComponent.SameAsLastEvent;
        }

        public static Map LastColonized() {
            return HumanStoryteller.StoryComponent.LastColonizedMap;
        }

        public static void SaveMapByName(String name, MapContainer container) {
            if (HumanStoryteller.StoryComponent.PawnBank.ContainsKey(name)) {
                RemoveByName(name);
            }

            HumanStoryteller.StoryComponent.MapBank.Add(name, container);
        }

        public static void RemoveByName(string name) {
            HumanStoryteller.StoryComponent.MapBank.Remove(name);
        }

        public static void RemoveByTile(long tile) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;

            foreach (var pair in mapBank) {
                if (pair.Value?.Parent.Tile == tile) {
                    mapBank.Remove(pair.Key);
                    return;
                }
            }
        }

        public static bool MapExists(Map map) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;
            foreach (var pair in mapBank) {
                if (pair.Value?.DecoupledMap == map) {
                    return true;
                }
            }

            return false;
        }
    }
}