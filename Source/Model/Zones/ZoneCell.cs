using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model.Zones {
    [JsonObject(MemberSerialization.OptIn)]
    public class ZoneCell {
        [JsonProperty] public long X;
        [JsonProperty] public long Z;

        public ZoneCell() {
        }

        public ZoneCell(IntVec3 cell) {
            X = cell.x;
            Z = cell.z;
        }

        public IntVec3 Pos {
            get => new IntVec3(Mathf.RoundToInt(X), 0, Mathf.RoundToInt(Z));
            set {
                X = value.x;
                Z = value.z;
            }
        }

        public void ApplyOffset(IntVec3 offset) {
            X += offset.x;
            Z += offset.z;
        }

        private bool Equals(ZoneCell other) {
            return Mathf.RoundToInt(X) == Mathf.RoundToInt(other.X)
                   && Mathf.RoundToInt(Z) == Mathf.RoundToInt(other.Z);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ZoneCell) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (X.GetHashCode() * 397) ^ Z.GetHashCode();
            }
        }
    }
}