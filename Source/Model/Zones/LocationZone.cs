﻿using System.Collections.Generic;
using HarmonyLib;
 using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
 using HumanStoryteller.Util.Logging;
 using Verse;

namespace HumanStoryteller.Model.Zones {
    public class LocationZone : IExposable  {
        [JsonProperty("C")] public List<ZoneCell> Cells;
        private Dictionary<long, Dictionary<long, ZoneCell>> _cachedLookup;

        public LocationZone() {
        }

        public LocationZone(List<ZoneCell> cells, IntVec3 origin) {
            _cachedLookup = null;
            Cells = Tell.AssertNotNull(cells, nameof(cells), GetType().Name);
            Tell.AssertNotNull(origin, nameof(origin), GetType().Name);
            ApplyOffset(new IntVec3(origin.x * -1, 0, origin.z * -1));
        }

        public LocationZone(List<ZoneCell> cells) {
            _cachedLookup = null;
            Cells = Tell.AssertNotNull(cells, nameof(cells), GetType().Name);
        }

        private LocationZone(List<ZoneCell> cells, Dictionary<long, Dictionary<long, ZoneCell>> cachedLookup) {
            _cachedLookup = cachedLookup;
            Cells = Tell.AssertNotNull(cells, nameof(cells), GetType().Name);
        }

        public void ApplyOffset(IntVec3 offset) {
            _cachedLookup = null;
            Cells.ForEach(c => c.ApplyOffset(offset));
        }

        public IntVec3 RandomCell(IntVec3 fallback) {
            return Cells.RandomElementWithFallback(new ZoneCell(fallback)).Pos;
        }

        public override string ToString() {
            return $"Cells: [{Cells.Join()}]";
        }

        public LocationZone DeepClone() {
            return new LocationZone(Cells.ConvertAll(input => new ZoneCell(input.Pos)), _cachedLookup);
        }

        public void CreateCacheLookup() {
            if (_cachedLookup == null) {
                CreateCacheLookupIntern();
            }
        }

        private void CreateCacheLookupIntern() {
            _cachedLookup = new Dictionary<long, Dictionary<long, ZoneCell>>();
            Cells.ForEach(zoneCell => {
                if (!_cachedLookup.ContainsKey(zoneCell.X)) {
                    _cachedLookup.Add(zoneCell.X, new Dictionary<long, ZoneCell>());
                }

                if (!_cachedLookup[zoneCell.X].ContainsKey(zoneCell.Z)) {
                    _cachedLookup[zoneCell.X].Add(zoneCell.Z, zoneCell);
                }
            });
        }
        
        public void ExposeData() {
            Scribe_Collections.Look(ref Cells, "cells", LookMode.Deep);
        }

        public bool Contains(IntVec3 cell) {
            if (_cachedLookup == null && Cells.Count > 1) {
                CreateCacheLookupIntern();
            }

            if (_cachedLookup == null) {
                return Cells.Contains(new ZoneCell(cell));
            }

            return _cachedLookup.ContainsKey(cell.x) && _cachedLookup[cell.x].ContainsKey(cell.z);
        }
    }
}