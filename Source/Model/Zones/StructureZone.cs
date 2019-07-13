using System.Collections.Generic;
using HumanStoryteller.Util;
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
            return $"Origin: {OriginX}:{OriginZ} Things: {Things.ToStringSafeEnumerable()}";
        }

        public void ExposeData() {
            Scribe_Collections.Look(ref Things, "things", LookMode.Deep);
            Scribe_Values.Look(ref OriginX, "originX");
            Scribe_Values.Look(ref OriginZ, "originZ");
        }
    }
}