using System.Collections.Generic;
using HumanStoryteller.Model.Zones;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model.Incident {
    public class Location : IExposable {
        private const string Zone = "Zone";
        private const string Preset = "Preset";
        private const string Pawn = "Pawn";
        private const string Precise = "Precise";
        
        private bool _hasCachedZone;
        private LocationZone _cachedZone;
        private IntVec3 _cachedOffsetForZone;

        public string Type = "";
        public string Value = "";
        public Location Offset;
        public Number X;
        public Number Z;

        public Location() {
        }

        public IntVec3 GetSingleCell(Map target, bool fallback = true, bool isOffset = false) {
            IntVec3 cell = GetCell(target);
            if (!fallback) {
                return cell;
            }

            if (isOffset) {
                return cell.IsValid ? cell : IntVec3.Zero;
            }

            return (!cell.IsValid ? DropCellFinder.RandomDropSpot(target) : cell).ClampInsideMap(target);
        }

        public LocationZone GetZone(Map target, bool zoneOnly = false) {
            if (isZone()) {
                var offset = Offset?.GetSingleCell(target, true, true) ?? IntVec3.Zero;
                if (!_hasCachedZone || !offset.Equals(_cachedOffsetForZone)) {
                    _cachedZone = AreaUtil.StringToLocationZone(Value, offset);
                    _hasCachedZone = true;
                    _cachedOffsetForZone = offset;
                }
                return _cachedZone;//.DeepClone();
            }

            return zoneOnly ? null : new LocationZone(new List<ZoneCell> {new ZoneCell(GetSingleCell(target))});
        }

        public bool isZone() {
            return Type == Zone;
        }

        public bool isPawn() {
            return Type == Pawn;
        }

        public bool isSet() {
            return Type != "";
        }

        private IntVec3 GetCell(Map target) {
            switch (Type) {
                case "":
                    return IntVec3.Invalid;
                case Preset:
                    return GetPresetCell(target);
                case Pawn:
                    return GetPawnCell(target);
                case Precise:
                    return GetPreciseCell();
                case Zone:
                    return GetZone(target, true)?.RandomCell(IntVec3.Invalid) ?? IntVec3.Invalid;
                default:
                    Tell.Warn($"During location parsing, found unknown type: [{Type}].");
                    return IntVec3.Invalid;
            }
        }

        private IntVec3 GetPresetCell(Map map) {
            switch (Value) {
                case "":
                    return IntVec3.Invalid;
                case "Pointer":
                    if (Current.Game.CurrentMap == map) {
                        return UI.MouseCell();
                    }

                    return IntVec3.Invalid;
                case "RandomEdge":
                    if (DropCellFinder.TryFindDropSpotNear(CellFinder.RandomEdgeCell(map), map, out var outResult, false, true)) {
                        return outResult;
                    } else {
                        Tell.Warn("No usable cell near edge");
                        return IntVec3.Invalid;
                    }

                case "Center":
                    if (RCellFinder.TryFindRandomCellNearWith(map.Center, x => true, map, out var result, 5, 500)) {
                        return result;
                    } else {
                        Tell.Warn("No usable cell near center");
                        return IntVec3.Invalid;
                    }
                case "OutsideColony":
                    if (RCellFinder.TryFindRandomSpotJustOutsideColony(map.Center, map, out var result2)) {
                        return result2;
                    } else {
                        Tell.Warn("No usable cell outside colony");
                        return IntVec3.Invalid;
                    }
                case "Siege":
                    return RCellFinder.FindSiegePositionFrom(map.Center, map);
                case "Random":
                    return DropCellFinder.RandomDropSpot(map);
                default:
                    Tell.Warn($"During location preset parsing, found unknown preset type: [{Value}].");
                    return IntVec3.Invalid;
            }
        }

        private IntVec3 GetPreciseCell() {
            var x = X?.GetValue() ?? 0;
            var z = Z?.GetValue() ?? 0;
            if (x == -1 || z == -1) {
                Tell.Warn($"Precise location seems invalid, X: [{x}] Z: [{z}]. Using anyway..");
            }

            return new IntVec3(Mathf.RoundToInt(x), 0, Mathf.RoundToInt(z));
        }

        private IntVec3 GetPawnCell(Map map) {
            Pawn p = PawnUtil.GetPawnByName(Value);
            if (p == null || p.Map != map && !p.Dead) {
                Tell.Warn($"Pawn with name {Value} not known or on target map.");
                return IntVec3.Invalid;
            }

            return p.Dead ? p.Corpse.Position : p.Position;
        }

        public override string ToString() {
            return $"Type: [{Type}], Value: [{Value}], X: [{X}], Z: [{Z}], Offset: [{Offset}]";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Type, "type");
            Scribe_Values.Look(ref Value, "value");
            Scribe_Deep.Look(ref Offset, "offset");
            Scribe_Deep.Look(ref X, "x");
            Scribe_Deep.Look(ref Z, "z");
        }
    }
}