using System;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Util {
    public class MapUtil {
        public static Map GetMapByName(String name, bool warn = true) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;

            if (mapBank.ContainsKey(name.ToUpper())) {
                var mapContainer = mapBank[name.ToUpper()];
                if (mapContainer == null) {
                    RemoveByName(name);
                    if (warn)
                        Tell.Warn("Requested map does not exist (anymore)", name);
                    return null;
                }
                var map = mapContainer.GetMap();
                if (map == null && warn) {
                    Tell.Warn("Requested map is not created yet (check first if map is created)", name);
                }
                
                return map;
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

            foreach (var pair in mapBank.Where(pair => pair.Value?.Parent.Tile == tile)) {
                if (pair.Value.IsDecoupled) {
                    return pair.Value;
                }

                var parent = pair.Value.Parent;
                if (parent == null && warn) {
                    Tell.Warn("Requested map is not created yet (check first if map is created)", tile);
                }

                return pair.Value;
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
            if (HumanStoryteller.StoryComponent.MapBank.ContainsKey(name.ToUpper())) {
                RemoveByName(name);
            }

            HumanStoryteller.StoryComponent.MapBank.Add(name.ToUpper(), container);
        }

        public static void RemoveByName(string name) {
            HumanStoryteller.StoryComponent.MapBank.Remove(name.ToUpper());
        }

        public static void RemoveByTile(long tile) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;

            foreach (var pair in mapBank.Where(pair => pair.Value?.Parent.Tile == tile)) {
                mapBank.Remove(pair.Key);
                return;
            }
        }

        public static bool MapExists(Map map) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;
            return mapBank.Any(pair => pair.Value?.DecoupledMap == map);
        }

        public static void AddPersistentMap(Map map) {
            HumanStoryteller.StoryComponent.PersistentMaps.Add(map);
        }

        public static bool CheckIfMapIsPersistent(Map map) {
            return HumanStoryteller.StoryComponent.PersistentMaps.Contains(map);
        }
    }
}