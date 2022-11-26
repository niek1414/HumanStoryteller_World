using System.Collections.Generic;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model.Zones; 
[JsonObject(MemberSerialization.OptIn)]
public class ZoneThing : IExposable {
    [JsonProperty("A")] [DefaultValue(-1)] public long X;
    [JsonProperty("B")] [DefaultValue(-1)] public long Z;
    [JsonProperty("C")] [DefaultValue(-1)] public long Rot;
    [JsonProperty("D")] [DefaultValue("")] public string DefType;
    [JsonProperty("E")] [DefaultValue(-1)] public long Points;
    [JsonProperty("F")] [DefaultValue(-1)] public long Quality;
    [JsonProperty("G")] [DefaultValue("")] public string Stuff;
    [JsonProperty("H")] [DefaultValue(-1)] public long Amount;
    [JsonProperty("I")] [DefaultValue("")] public string Faction;
    [JsonProperty("J")] [DefaultValue("")] public string Type;
    [JsonProperty("K")] [DefaultValue(-1)] public float Gauge;
    [JsonProperty("L")] [DefaultValue(-1)] public long Age;
    [JsonProperty("M")] [DefaultValue(-1)] public float Growth;
    [JsonProperty("O")] [DefaultValue(-1)] public float Depth;

    [JsonProperty("P")] [DefaultValue(null)]
    public List<ZoneThing> Equipment;

    [JsonProperty("Q")] [DefaultValue(null)]
    public List<ZoneThing> Apparel;

    public ZoneThing() {
        X = -1;
        Z = -1;
        Rot = -1;
        Points = -1;
        Quality = -1;
        Amount = -1;
        Gauge = -1;
        Age = -1;
        Growth = -1;
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

    public ZoneThing(int x, int z, Def defType, int points, int age, float growth)
        : this(x, z, defType?.defName, null) {
        Type = "V";
        Age = age;
        Growth = growth;
        Points = points;
    }

    public ZoneThing(int x, int z, TerrainDef defType)
        : this(x, z, defType?.defName, null) {
        Type = "F";
    }

    public ZoneThing(int x, int z, RoofDef defType)
        : this(x, z, defType?.defName, null) {
        Type = "R";
    }

    public ZoneThing(int x, int z, float depth)
        : this(x, z, null, null) {
        Type = "S";
        Depth = depth;
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

    public bool IsPlant() {
        return Type == "V";
    }

    public bool IsRoof() {
        return Type == "R";
    }

    public bool IsSnow() {
        return Type == "S";
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
        Scribe_Values.Look(ref Type, "type");
        Scribe_Values.Look(ref Gauge, "gauge");
        Scribe_Values.Look(ref Age, "age");
        Scribe_Values.Look(ref Growth, "growth");
        Scribe_Values.Look(ref Depth, "depth");
        Scribe_Collections.Look(ref Equipment, "Equipment", LookMode.Deep);
        Scribe_Collections.Look(ref Apparel, "Apparel", LookMode.Deep);
    }

    public override string ToString() {
        return
            $"X: {X}, Z: {Z}, Rot: {Rot}, DefType: {DefType}, Points: {Points}, Quality: {Quality}, Stuff: {Stuff}, Amount: {Amount}, Faction: {Faction}, Type: {Type}, Gauge: {Gauge}, Age: {Age}, Growth: {Growth}, Depth: {Depth}, Equipment: [{Equipment.ToStringSafeEnumerable()}], Apparel: [{Apparel.ToStringSafeEnumerable()}]";
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
        get => Faction == null ? null : FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed(Faction, false));
        set => Faction = value?.def?.defName;
    }

    public Def DefTypeObj {
        get {
            if (IsFloor()) {
                return DefDatabase<TerrainDef>.GetNamed(DefType, false);
            }

            if (IsPlant()) {
                return DefDatabase<ThingDef>.GetNamed(DefType, false);
            }

            if (IsRoof()) {
                return DefDatabase<RoofDef>.GetNamed(DefType, false);
            }

            if (IsSnow()) {
                return null;
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