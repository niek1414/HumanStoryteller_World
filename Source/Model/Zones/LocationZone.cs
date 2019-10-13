using System.Collections.Generic;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Newtonsoft.Json;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model.Zones {
    public class LocationZone : IExposable {
        [JsonProperty("C")] public List<ZoneCell> Cells;

        public LocationZone() {
        }

        public LocationZone(List<ZoneCell> cells, IntVec3 origin) {
            Cells = Tell.AssertNotNull(cells, nameof(cells), GetType().Name);
            Tell.AssertNotNull(origin, nameof(origin), GetType().Name);
            ApplyOffset(new IntVec3(origin.x * -1, 0, origin.z * -1));
        }

        public LocationZone(List<ZoneCell> cells) {
            Cells = Tell.AssertNotNull(cells, nameof(cells), GetType().Name);
        }

        public void ApplyOffset(IntVec3 offset) {
            Cells.ForEach(c => c.ApplyOffset(offset));
        }

        public IntVec3 RandomCell(IntVec3 fallback) {
            return Cells.RandomElementWithFallback(new ZoneCell(fallback)).Pos;
        }

        public override string ToString() {
            return $"Cells: {Cells.ToStringSafeEnumerable()}";
        }

        public void ExposeData() {
            Scribe_Collections.Look(ref Cells, "cells", LookMode.Deep);
        }

        public bool Contains(IntVec3 cell) {
            return Cells.Contains(new ZoneCell(cell));
        }
    }
}