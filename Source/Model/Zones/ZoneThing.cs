using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model.Zones {
    [JsonObject(MemberSerialization.OptIn)]
    public class ZoneThing : IExposable {
        [JsonProperty("X")][DefaultValue(-1)] public long X;
        [JsonProperty("Z")][DefaultValue(-1)] public long Z;
        [JsonProperty("R")][DefaultValue(-1)] public long Rot;
        [JsonProperty("D")][DefaultValue("")] public string DefType;
        [JsonProperty("P")][DefaultValue(-1)] public long Points;
        [JsonProperty("Q")][DefaultValue(-1)] public long Quality;
        [JsonProperty("S")][DefaultValue("")] public string Stuff;
        [JsonProperty("A")][DefaultValue(-1)] public long Amount;
        [JsonProperty("F")][DefaultValue("")] public string Faction;
        [JsonProperty("T")][DefaultValue("")] public string Type;
        [JsonProperty("G")][DefaultValue(-1)] public float Gauge;
        [JsonProperty("E")][DefaultValue(null)] public List<ZoneThing> Equipment;
        [JsonProperty("AP")][DefaultValue(null)] public List<ZoneThing> Apparel;

        public ZoneThing() {
            X = -1;
            Z = -1;
            Rot = -1;
            Points = -1;
            Quality = -1;
            Amount = -1;
            Gauge = -1;
        }

        public ZoneThing(int x, int z, Def kindDef, int points, Faction owner, string name, List<ZoneThing> equipment, List<ZoneThing> apparel) 
            : this(x, z, kindDef?.defName, name) {
            Type = "P";
            Points = points;
            FactionObj = owner;
            Equipment = equipment.Count > 0 ? equipment : null;
            Apparel = apparel.Count > 0 ? apparel : null;
        }
        
        public ZoneThing(int x, int z, Rot4 rotObj, Def defType, int points, QualityCategory qualityCategory, Def stuff, Faction owner, float gauge) 
            : this(x, z, defType?.defName, stuff?.defName) {
            Type = "B";
            RotObj = rotObj;
            Points = points;
            QualityCategory = qualityCategory;
            FactionObj = owner ?? RimWorld.Faction.OfPlayer;
            Gauge = gauge;
        }

        public ZoneThing(int x, int z, Def defType, int points, QualityCategory qualityCategory, Def stuff, int amount, Faction owner) 
            : this(x, z, defType?.defName, stuff?.defName) {
            Type = "I";
            Amount = amount;
            Points = points;
            QualityCategory = qualityCategory;
            FactionObj = owner ?? RimWorld.Faction.OfPlayer;
        }

        public ZoneThing(int x, int z, Def defType) 
            : this(x, z, defType?.defName, null) {
            Type = "F";
        }

        private ZoneThing(int x, int z, string defType, string stuff) : this() {
            X = x;
            Z = z;
            DefType = defType;
            Stuff = stuff ?? "";
        }

        public bool IsBuilding() {
            return Type == "B";
        }

        public bool IsItem() {
            return Type == "I";
        }

        public bool IsFloor() {
            return Type == "F";
        }

        public bool IsPawn() {
            return Type == "P";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref X, "x");
            Scribe_Values.Look(ref Z, "z");
            Scribe_Values.Look(ref Rot, "rot");
            Scribe_Values.Look(ref DefType, "defType");
            Scribe_Values.Look(ref Points, "points");
            Scribe_Values.Look(ref Quality, "quality");
            Scribe_Values.Look(ref Stuff, "stuff");
            Scribe_Values.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Faction, "faction");
        }

        public override string ToString() {
            return $"X: {X}, Z: {Z}, Rot: {Rot}, DefType: {DefType}, Points: {Points}, Quality: {Quality}, Stuff: {Stuff}, Amount: {Amount}, Faction: {Faction}, Type: {Type}";
        }

        public IntVec3 GetCellLocation(StructureZone parent, IntVec3 offset) {
            return new IntVec3(
                Mathf.RoundToInt(X) - Mathf.RoundToInt(parent.OriginX) + offset.x, 0,
                Mathf.RoundToInt(Z) - Mathf.RoundToInt(parent.OriginZ) + offset.z);
        }
        
        public Rot4 RotObj {
            get => Rot == -1 ? Rot4.North : new Rot4(Mathf.RoundToInt(Rot));
            set => Rot = value.AsInt;
        }

        public Faction FactionObj {
            get => FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed(Faction, false));
            set => Faction = value.def.defName;
        }

        public Def DefTypeObj {
            get {
                if (IsFloor()) {
                    return DefDatabase<TerrainDef>.GetNamed(DefType, false);
                }
                if (IsPawn()) {
                    return DefDatabase<PawnKindDef>.GetNamed(DefType, false);
                }
                return DefDatabase<ThingDef>.GetNamed(DefType, false);
            }
        }

        public ThingDef StuffObj => DefDatabase<ThingDef>.GetNamed(Stuff, false);

        public QualityCategory QualityCategory {
            get {
                switch (Quality) {
                    case 0:
                        return QualityCategory.Awful;
                    case 1:
                        return QualityCategory.Poor;
                    case 2:
                        return QualityCategory.Normal;
                    case 3:
                        return QualityCategory.Good;
                    case 4:
                        return QualityCategory.Excellent;
                    case 5:
                        return QualityCategory.Masterwork;
                    default:
                        return QualityUtility.GenerateQualityRandomEqualChance();
                }
            }
            set {
                switch (value) {
                    case QualityCategory.Awful:
                        Quality = 0;
                        break;
                    case QualityCategory.Poor:
                        Quality = 1;
                        break;
                    case QualityCategory.Normal:
                        Quality = 2;
                        break;
                    case QualityCategory.Good:
                        Quality = 3;
                        break;
                    case QualityCategory.Excellent:
                        Quality = 4;
                        break;
                    case QualityCategory.Masterwork:
                        Quality = 5;
                        break;
                    default:
                        Quality = -1;
                        break;
                }
            }
        }
    }
}