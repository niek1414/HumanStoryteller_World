using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model.Action;
using HumanStoryteller.Model.Zones;
using HumanStoryteller.Parser.Converter;
using HumanStoryteller.Util.Logging;
using Newtonsoft.Json;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util {
    public class AreaUtil {
        public static string AreaLocationToString(IntVec3 origin) {
            var currentMap = Current.Game.CurrentMap;
            Area_Home zoneToCopy = currentMap.areaManager.Home;
            if (zoneToCopy == null) {
                Tell.Warn("ZoneToCopy was null");
                return "";
            }

            List<ZoneCell> zoneCells = new List<ZoneCell>();

            foreach (IntVec3 cell in zoneToCopy.ActiveCells) {
                zoneCells.Add(new ZoneCell(cell));
            }

            LocationZone root = new LocationZone(zoneCells, origin);
            var str = JsonConvert.SerializeObject(root, Formatting.None,
                new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore});
            var compStr = ZipUtil.Zip(str);

            return str.Length <= compStr.Length ? str : compStr;
        }

        public static LocationZone StringToLocationZone(string compressedJson, IntVec3 offset) {
            var json = ZipUtil.Unzip(compressedJson);
            var settings = new JsonSerializerSettings
                {NullValueHandling = NullValueHandling.Ignore, Converters = new List<JsonConverter> {new DecimalJsonConverter()}};
            try {
                var zone = JsonConvert.DeserializeObject<LocationZone>(json, settings);
                zone.ApplyOffset(offset);
                zone.CreateCacheLookup();
                return zone;
            } catch (JsonSerializationException e) {
                Tell.Err($"Unable to parse location zone, {e.Message}", e);
                return null;
            }
        }

        public static string AreaObjectsToString(IntVec3 origin, bool buildings, bool items, bool terrain, bool floor, bool pawns, bool plants,
            bool roof) {
            var currentMap = Current.Game.CurrentMap;
            Area_Home zoneToCopy = currentMap.areaManager.Home;
            if (zoneToCopy == null) {
                Tell.Warn("ZoneToCopy was null");
                return "";
            }

            List<ZoneThing> zoneThings =
                (from x in currentMap.listerThings.AllThings
                    where (x.def.category == ThingCategory.Item || x.def.category == ThingCategory.Building ||
                           x.def.category == ThingCategory.Pawn || x.def.category == ThingCategory.Plant) && zoneToCopy[x.Position]
                    select x).Select(thing => ItemToZoneThing(thing, buildings, items, pawns, plants)).Where(item => item != null).ToList();

            if (floor || terrain) {
                for (var i = 0; i < currentMap.terrainGrid.topGrid.Length; i++) {
                    if (!zoneToCopy[i]) {
                        continue;
                    }

                    var def = currentMap.terrainGrid.topGrid[i];
                    if ((!def.BuildableByPlayer || !floor) && (def.BuildableByPlayer || !terrain)) continue;
                    IntVec3 loc = currentMap.cellIndices.IndexToCell(i);
                    zoneThings.Add(new ZoneThing(loc.x, loc.z, def));
                }
            }

            if (terrain) {
                var depthGrid = (float[]) Traverse.Create(currentMap.snowGrid).Field("depthGrid").GetValue();
                for (var i = 0; i < depthGrid.Length; i++) {
                    if (!zoneToCopy[i]) {
                        continue;
                    }

                    var depth = depthGrid[i];
                    if (depth <= 0.0) continue;
                    IntVec3 loc = currentMap.cellIndices.IndexToCell(i);
                    zoneThings.Add(new ZoneThing(loc.x, loc.z, depth));
                }
            }

            if (roof) {
                var roofGrid = (RoofDef[]) Traverse.Create(currentMap.roofGrid).Field("roofGrid").GetValue();
                for (var i = 0; i < roofGrid.Length; i++) {
                    if (!zoneToCopy[i]) {
                        continue;
                    }

                    var def = roofGrid[i];
                    if (def == null) continue;
                    IntVec3 loc = currentMap.cellIndices.IndexToCell(i);
                    zoneThings.Add(new ZoneThing(loc.x, loc.z, def));
                }
            }

            StructureZone root = new StructureZone(zoneThings, origin);
            var str = JsonConvert.SerializeObject(root);
            var compStr = ZipUtil.Zip(str);

            return str.Length <= compStr.Length ? str : compStr;
        }

        private static ZoneThing ItemToZoneThing(Thing thing, bool buildings, bool items, bool pawns, bool plants) {
            if (thing is Corpse) {
                Tell.Warn("Not yet supporting corpses as export");
                return null;
            }

            switch (thing.def.category) {
                case ThingCategory.Building: {
                    if (!buildings) return null;
                    thing.TryGetQuality(out var qc);
                    CompRefuelable fuelComp = thing.TryGetComp<CompRefuelable>();
                    CompPowerBattery powerComp = thing.TryGetComp<CompPowerBattery>();
                    return new ZoneThing(thing.Position.x, thing.Position.z, thing.Rotation, thing.def, thing.HitPoints, qc, thing.Stuff,
                        thing.Faction, fuelComp?.Fuel ?? powerComp?.StoredEnergy ?? -1);
                }
                case ThingCategory.Item: {
                    if (!items) return null;
                    thing.TryGetQuality(out var qc);
                    return new ZoneThing(thing.Position.x, thing.Position.z, thing.def, thing.HitPoints, qc, thing.Stuff,
                        thing.stackCount, thing.Faction);
                }
                case ThingCategory.Plant: {
                    if (!plants || !(thing is Plant p)) return null;
                    return new ZoneThing(p.Position.x, p.Position.z, p.def, p.HitPoints, p.Age, p.Growth);
                }
                case ThingCategory.Pawn: {
                    if (!pawns || !(thing is Pawn p)) return null;
                    string name;
                    if (p.Name != null) {
                        switch (p.Name) {
                            case NameTriple n:
                                name = n.First + "::" + n.Nick + "::" + n.Last;
                                break;
                            case NameSingle n:
                                name = n.Name;
                                break;
                            default:
                                name = p.Name.ToStringFull;
                                break;
                        }
                    } else {
                        name = "";
                    }

                    var equipment = new List<ZoneThing>();
                    p.equipment?.AllEquipmentListForReading?.ForEach(i => {
                        var item = ItemToZoneThing(i, false, true, false, false);
                        if (item != null) {
                            equipment.Add(item);
                        }
                    });

                    var apparel = new List<ZoneThing>();
                    p.apparel?.WornApparel?.ForEach(i => {
                        var item = ItemToZoneThing(i, false, true, false, false);
                        if (item != null) {
                            apparel.Add(item);
                        }
                    });
                    return new ZoneThing(thing.Position.x, thing.Position.z, p.kindDef, thing.HitPoints, p.Faction, name, equipment, apparel);
                }
                default:
                    return null;
            }
        }

        public static bool StringToAreaObjects(string compressedJson, Map target, IntVec3 offset, IncidentResult_CreatedStructure ir) {
            string json;
            try {
                json = ZipUtil.Unzip(compressedJson);
            } catch (Exception e) {
                Tell.Warn(e.Message + " __stack: " + e.StackTrace);
                return false;
            }

            try {
                var settings = new JsonSerializerSettings
                    {NullValueHandling = NullValueHandling.Ignore, Converters = new List<JsonConverter> {new DecimalJsonConverter()}};
                StructureZone root = JsonConvert.DeserializeObject<StructureZone>(json, settings);
                HumanStoryteller.StoryComponent.StoryQueue.Add(new SpawnItemAction(root, target, offset, ir));
                return true;
            } catch (Exception e) {
                Tell.Warn(e.Message + " __json: " + json + " __stack: " + e.StackTrace);
                return false;
            }
        }

        public static void FloodStructureZone(Map target, StructureZone root, IntVec3 offset) {
            CellIndices cellIndices = target.cellIndices;
            if (target.fogGrid.fogGrid == null) {
                target.fogGrid.fogGrid = new bool[cellIndices.NumGridCells];
            }

            bool[] previousGrid = (bool[]) target.fogGrid.fogGrid.Clone();
            for (var i = 0; i < target.fogGrid.fogGrid.Length; i++) {
                target.fogGrid.fogGrid[i] = true;
            }

            foreach (var pawn in target.mapPawns.PawnsInFaction(Faction.OfPlayer)) {
                FloodFillerFog.FloodUnfog(pawn.Position, target);
            }

            var newGrid = target.fogGrid.fogGrid;
            foreach (var thing in root.Things) {
                var cell = thing.GetCellLocation(root, offset);
                if (!cell.InBounds(target)) {
                    continue;
                }

                var i = cellIndices.CellToIndex(cell);
                target.fogGrid.fogGrid = previousGrid;
                target.fogGrid.fogGrid[i] = newGrid[i];
                target.mapDrawer.MapMeshDirty(cell, MapMeshFlag.FogOfWar);
            }

            target.roofGrid.Drawer.SetDirty();
        }

        public static Thing SpawnThing(ZoneThing thing, StructureZone root, Map target, IntVec3 offset, bool spawn = true) {
            if (thing == null) {
                Tell.Warn("Found empty object while creating structure");
                return null;
            }

            Thing spawnThing;
            IntVec3 newLoc = thing.GetCellLocation(root, offset);
            if (!newLoc.InBounds(target) && spawn) {
                Tell.Log("Trying to spawn thing outside of the map", newLoc);
                return null;
            }

            Def def = null;
            if (!thing.IsSnow()) {
                def = thing.DefTypeObj;
                if (def == null) {
                    Tell.Warn("Found object with no Def while creating structure");
                    return null;
                }
            }

            ThingDef stuffDef = thing.StuffObj;

            if (thing.IsBuilding()) {
                var thingDef = (ThingDef) def;

                spawnThing = ThingMaker.MakeThing(thingDef, CorrectStuffForThingDef(thingDef, stuffDef));
                if (thingDef.CanHaveFaction) {
                    var faction = thing.FactionObj;
                    spawnThing.SetFactionDirect(faction);
                }

                CompRefuelable fuelComp = spawnThing.TryGetComp<CompRefuelable>();
                CompPowerBattery powerComp = spawnThing.TryGetComp<CompPowerBattery>();
                if (thing.Gauge != -1) {
                    fuelComp?.ConsumeFuel(fuelComp.Fuel);
                    fuelComp?.Refuel(thing.Gauge);
                    Traverse.Create(powerComp).Field("storedEnergy").SetValue(thing.Gauge);
                }
            } else if (thing.IsItem()) {
                var thingDef = (ThingDef) def;
                spawnThing = ThingMaker.MakeThing(thingDef, CorrectStuffForThingDef(thingDef, stuffDef));
                spawnThing.stackCount = Mathf.RoundToInt(thing.Amount);
                spawnThing = ItemUtil.TryMakeMinified(spawnThing);
                if (thingDef.CanHaveFaction) {
                    GraphicDatabase.AllGraphicsLoaded();
                    var faction = thing.FactionObj;
                    spawnThing.SetFactionDirect(faction);
                }
            } else if (thing.IsPlant()) {
                var thingDef = (ThingDef) def;
                var plant = (Plant) ThingMaker.MakeThing(thingDef);

                if (spawn) {
                    GenSpawn.Spawn(plant, newLoc, target);
                }

                return plant;
            } else if (thing.IsFloor()) {
                if (spawn) {
                    target.terrainGrid.SetTerrain(newLoc, (TerrainDef) def);
                }

                return new Thing();
            } else if (thing.IsRoof()) {
                if (spawn) {
                    target.roofGrid.SetRoof(newLoc, (RoofDef) def);
                }

                return new Thing();
            } else if (thing.IsSnow()) {
                if (spawn) {
                    target.snowGrid.SetDepth(newLoc, thing.Depth);
                }

                return new Thing();
            } else if (thing.IsPawn()) {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    (PawnKindDef) thing.DefTypeObj,
                    thing.FactionObj,
                    PawnGenerationContext.NonPlayer,
                    target.Tile,
                    false, false, false, false, true, true, 1F, false, true, false
                ));

                if (thing.Stuff != "") {
                    Name n;
                    if (thing.Stuff.Contains("::")) {
                        var split = thing.Stuff.Split(':', ':');
                        n = new NameTriple(split[0], split[1] == "" ? split[0] : split[1], split[2]);
                    } else {
                        n = new NameSingle(thing.Stuff);
                    }

                    switch (pawn.Name) {
                        case NameSingle _:
                            pawn.Name = n.GetType() == typeof(NameSingle) ? n : new NameSingle(n.ToStringFull);
                            break;
                        default:
                            pawn.Name = n.GetType() == typeof(NameTriple) ? n : new NameTriple(n.ToStringFull, "", "");
                            break;
                    }
                }

                pawn.equipment?.DestroyAllEquipment();
                thing.Equipment?.ForEach(zt => {
                    var equipment = (ThingWithComps) SpawnThing(zt, root, target, offset, false);
                    if (equipment == null) {
                        Tell.Warn("While equipping pawn, an null equipment was found:", zt);
                    }

                    pawn.equipment.AddEquipment(equipment);
                });

                pawn.apparel?.DestroyAll();
                thing.Apparel?.ForEach(zt => {
                    var apparel = (Apparel) SpawnThing(zt, root, target, offset, false);
                    if (apparel == null) {
                        Tell.Warn("While equipping pawn, an null apparel was found:", zt);
                    }

                    pawn.apparel.Wear(apparel, false);
                });

                if (spawn) {
                    GenSpawn.Spawn(pawn, newLoc, target);
                    if (pawn.Faction != Faction.OfPlayer && pawn.Faction != null) {
                        var fakeParms = new IncidentParms {
                            faction = pawn.Faction,
                            target = target,
                            pawnGroups = new Dictionary<Pawn, int> {{pawn, 0}},
                            raidStrategy = RaidStrategyDefOf.ImmediateAttack
                        };
                        RaidStrategyDefOf.ImmediateAttack.Worker.MakeLords(fakeParms, new List<Pawn> {pawn});
                    }
                }

                return pawn;
            } else {
                Tell.Warn("Found thing of unknown type:" + thing);
                return null;
            }

            ItemUtil.TrySetQuality(spawnThing, thing.QualityCategory);
            spawnThing.HitPoints = Mathf.RoundToInt(thing.Points);
            if (spawn) {
                GenSpawn.Spawn(spawnThing, newLoc, target, thing.RotObj);
            }

            return spawnThing;
        }

        private static ThingDef CorrectStuffForThingDef(ThingDef thing, ThingDef stuff) {
            if (stuff != null && !stuff.IsStuff) {
                return GenStuff.DefaultStuffFor(thing);
            }

            if (thing.MadeFromStuff && stuff == null) {
                return GenStuff.DefaultStuffFor(thing);
            }

            if (!thing.MadeFromStuff && stuff != null) {
                return null;
            }

            return stuff;
        }
    }
}