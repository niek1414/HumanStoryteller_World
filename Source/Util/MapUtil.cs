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

        public static IntVec3 FindLocationByName(string loc, Map map) {
            switch (loc) {
                case "RandomEdge":
                    if (DropCellFinder.TryFindDropSpotNear(CellFinder.RandomEdgeCell(map), map, out var outResult, false, true)) {
                        return outResult;
                    } else {
                        return DropCellFinder.RandomDropSpot(map);
                    }

                case "Center":
                    return RCellFinder.TryFindRandomCellNearWith(map.Center, null, map, out var result)
                        ? result
                        : DropCellFinder.RandomDropSpot(map);
                case "OutsideColony":
                    return RCellFinder.TryFindRandomSpotJustOutsideColony(map.Center, map, out var result2)
                        ? result2
                        : DropCellFinder.RandomDropSpot(map);
                case "Siege":
                    return RCellFinder.FindSiegePositionFrom(map.Center, map);
                case "Random":
                    return DropCellFinder.RandomDropSpot(map);
                default:
                    string[] splinted = loc.Split(':');
                    if (splinted.Length == 3) {
                        if (!int.TryParse(splinted[0], out var x)
                            || !int.TryParse(splinted[1], out var y)
                            || !int.TryParse(splinted[2], out var z)) {
                            Tell.Warn("Tried to parse location with :'s but got x:" + splinted[0] + " y:" + splinted[1] + " z:" + splinted[2]);
                            return DropCellFinder.RandomDropSpot(map);
                        }
                        return new IntVec3(x, y, z);
                    }

                    Pawn p = PawnUtil.GetPawnByName(loc);
                    if (p == null || p.Map != map) {
                        return DropCellFinder.RandomDropSpot(map);
                    }

                    return p.Position;
            }
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