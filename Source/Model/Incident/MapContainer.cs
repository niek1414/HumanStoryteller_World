using System;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Model.Incident {
    public class MapContainer : IExposable {
        public bool FakeConnected;
        public MapParent Parent;
        public Map DecoupledMap;

        public MapContainer() {
        }

        public MapContainer(MapParent parent, Map decoupledMap = null) {
            Parent = parent;
            DecoupledMap = decoupledMap;

            if (IsDecoupled && parent.HasMap) {
                Tell.Warn("Initialized map container with decoupled map while parent also has a map, discarding decoupled map...");
                DecoupledMap = null;
            }
        }

        public Map GetMap(bool noDecouple = false) {
            try {
                if (IsDecoupled) {
                    return DecoupledMap;
                }

                if (Parent.HasMap) {
                    return Parent.Map;
                }

                Tell.Log("Map not found, trying to generate based on parent...");
                var map = GetOrGenerateMapUtility.GetOrGenerateMap(Parent.Tile, Parent.def);

                if (map == null || map.uniqueID == -1) {
                    Tell.Err("Tried to generate map but failed. Returning 'null'!");
                } else if (!noDecouple) {
                    Decouple(map);
                }

                return map;
            } catch (Exception e) {
                Tell.Err("Error occured during map retrieval: " + e.Message, e);
                throw;
            }
        }

        public bool IsDecoupled => DecoupledMap != null;

        public void ExposeData() {
            if (Scribe.mode == LoadSaveMode.Saving) {
                FakeConnect();
                Scribe_Values.Look(ref FakeConnected, "fakeConnected");
                Scribe_References.Look(ref Parent, "parent");
            } else {
                Scribe_Values.Look(ref FakeConnected, "fakeConnected");
                Scribe_References.Look(ref Parent, "parent");
            }
        }

        public void ExposeDataAfter() {
            try {
                Parent.Map?.FinalizeLoading();
            } catch (NullReferenceException e) {
                Tell.Warn("Error while finalize loading on map from map container", e);
            }
            FakeDisconnect();
        }

        public void TryCouple() {
            if (FakeConnected) {
                Tell.Err("Map is locked. Fake connection active. Trying to restore..");
                FakeDisconnect();
                return;
            }

            if (!IsDecoupled) {
                Tell.Log("Tried to couple map but is not decoupled, tile:" + Parent.Tile);
                return;
            }

            var map = GetMap();
            if (map == null) {
                Tell.Warn("Cannot find nor create map, tile: " + Parent.Tile);
                return;
            }

            Current.Game.AddMap(map);
            map.listerThings.AllThings.ForEach(t => Current.Game.tickManager.RegisterAllTickabilityFor(t));
            DecoupledMap = null;
        }

        public void Decouple(Map map = null) {
            if (FakeConnected) {
                Tell.Err("Map is locked. Fake connection active. Trying to restore..");
                FakeDisconnect();
            }

            if (IsDecoupled) {
                Tell.Log("Tried to decouple already decoupled map", DecoupledMap.ToString());
                return;
            }

            DecoupledMap = map ?? Parent.Map ?? GetOrGenerateMapUtility.GetOrGenerateMap(Parent.Tile, Parent.def);
            if (DecoupledMap == null) {
                Tell.Err("Failed to decouple map, parent has no map and generation failed");
                return;
            }

            //Deregister all things from tick
            Find.TickManager.RemoveAllFromMap(DecoupledMap);

            //Finish power tasks
            DecoupledMap.powerNetManager.UpdatePowerNetsAndConnections_First();
            
            Map currentMap = Current.Game.CurrentMap;
            Current.Game.Maps.Remove(DecoupledMap);
            if (currentMap != null)
            {
                sbyte b = (sbyte)Current.Game.Maps.IndexOf(currentMap);
                if (b < 0)
                {
                    if (Current.Game.Maps.Any())
                    {
                        Current.Game.CurrentMap = Current.Game.Maps[0];
                    }
                    else
                    {
                        Current.Game.CurrentMap = null;
                    }
                    Find.World.renderer.wantedMode = WorldRenderMode.Planet;
                }
                else
                {
                    Current.Game.currentMapIndex = b;
                }
            }
            if (Current.ProgramState == ProgramState.Playing)
            {
                Find.ColonistBar.MarkColonistsDirty();
            }
        }

        public void Remove() {
            FakeDisconnect();
            if (IsDecoupled) {
                MapUtil.RemoveByTile(Parent.Tile);
            } else if (Parent.Map != null) {
                Current.Game.DeinitAndRemoveMap(Parent.Map);
            }
        }

        public void FakeConnect() {
            if (FakeConnected || !IsDecoupled) return;
            Current.Game.Maps.Add(GetMap());
            FakeConnected = true;
        }

        public void FakeDisconnect() {
            if (!FakeConnected) return;
            FakeConnected = false;
            Map m = Parent.Map;
            if (m == null) return;
            Find.TickManager.RemoveAllFromMap(m);
            m.powerNetManager.UpdatePowerNetsAndConnections_First();
            Current.Game.Maps.Remove(m);
            DecoupledMap = m;
        }

        public override string ToString() {
            return $"FakeConnected: [{FakeConnected}], Parent: [{Parent}], DecoupledMap: [{DecoupledMap}], IsDecoupled: [{IsDecoupled}]";
        }
    }
}