using System;
using System.Collections.Generic;
using System.Linq;
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
            return HumanStoryteller.StoryComponent.FirstMapOfPlayer;
        }

        public static Map SameAsLastEvent() {
            return HumanStoryteller.StoryComponent.SameAsLastEvent;
        }

        public static Map LastColonized() {
            return HumanStoryteller.StoryComponent.LastColonizedMap;
        }

        public static void SaveMapByName(String name, MapParent map) {
            if (HumanStoryteller.StoryComponent.PawnBank.ContainsKey(name)) {
                RemoveName(name);
            }

            HumanStoryteller.StoryComponent.MapBank.Add(name, map);
        }

        public static void RemoveName(string name) {
            HumanStoryteller.StoryComponent.MapBank.Remove(name);
        }

        public static bool MapExists(Map map) {
            var mapBank = HumanStoryteller.StoryComponent.MapBank;
            foreach (var pair in mapBank) {
                if (pair.Value?.Map == map) {
                    return true;
                }
            }

            return false;
        }
        
        public static Map GetTarget(string target) {
            switch (target) {
                case "FirstOfPlayer":
                    return FirstOfPlayer();
                case "RandomOfPlayer":
                    return Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
                case "SameAsLastEvent":
                    return SameAsLastEvent();
                case "LastColonized":
                    return LastColonized();
                default: // With name?
                    return GetMapByName(target) ?? FirstOfPlayer();
            }
        }
    }
}