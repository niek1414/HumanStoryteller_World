﻿using System;
using System.Collections.Generic;
using Harmony;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.Zones {
    public class StructureZone : IExposable {
        public List<ZoneThing> Things;
        public long OriginX;
        public long OriginZ;

        public StructureZone() {
        }

        public StructureZone(List<ZoneThing> things, IntVec3 origin) {
            Things = Tell.AssertNotNull(things, nameof(things), GetType().Name);
            Tell.AssertNotNull(origin, nameof(origin), GetType().Name);

            OriginX = origin.x;
            OriginZ = origin.z;
        }

        public StructureZone(List<ZoneThing> things) {
            Things = Tell.AssertNotNull(things, nameof(things), GetType().Name);
            OriginX = 0;
            OriginZ = 0;
        }

        public override string ToString() {
            if (HumanStoryteller.DEBUG) {
                return $"Origin: [{OriginX}]:{OriginZ} Things: [{Things.Join()}]";
            }
            return $"Origin: [{OriginX}]:{OriginZ} Things (printing amount): [{Things.Count}]";
        }

        public void ExposeData() {
            Scribe_Collections.Look(ref Things, "things", LookMode.Deep);
            Scribe_Values.Look(ref OriginX, "originX");
            Scribe_Values.Look(ref OriginZ, "originZ");
        }

        public List<IntVec3> BoundBox(IntVec3 offset) {
            var result = new List<IntVec3>();
            var xMin = Things[0].X - 1;
            var xMax = Things[0].X + 1;
            var yMin = Things[0].Z - 1;
            var yMax = Things[0].Z + 1;
            for (var i = 1; i < Things.Count; i++) {
                var thing = Things[i];
                xMin = Math.Min(xMin, thing.X);
                xMax = Math.Max(xMax, thing.X);
                yMin = Math.Min(yMin, thing.Z);
                yMax = Math.Max(yMax, thing.Z);
            }

            for (var x = (int) xMin; x <= xMax; x++) {
                for (var z = (int) yMin; z <= yMax; z++) {
                    result.Add(new IntVec3(
                        (int) (x - OriginX + offset.x), 0,
                        (int) (z - OriginZ + offset.z)));
                }
            }

            return result;
        }
    }
}