using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model.Zones;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.Action {
    public class SpawnItemAction : IStoryAction {
        private const int MaxItemsPerTick = 50;
        
        public int FailCounter;
        public int Spawned;
        public StructureZone Root;
        public Map Target;
        public IntVec3 Offset;
        public IncidentResult_CreatedStructure Ir;

        private MapContainer _container;
        private bool _triedForContainer;

        public SpawnItemAction(StructureZone root, Map target, IntVec3 offset, IncidentResult_CreatedStructure ir) {
            Root = root;
            Target = target;
            Offset = offset;
            Ir = ir;
        }

        public void Action() {
            HumanStoryteller.StoryComponent.StoryStatus.CreatingStructure = true;
            if (!_triedForContainer) {
                _triedForContainer = true;
                _container = MapUtil.GetMapContainerByTile(Target.Tile, false);
            }
            var autoHomeArea = Find.PlaySettings.autoHomeArea;
            Find.PlaySettings.autoHomeArea = false;
            Target.regionAndRoomUpdater.Enabled = false;

            var currentMax = Spawned + MaxItemsPerTick;
            var end = false;
            for (var i = Spawned; i < currentMax; i++) {
                if (i >= Root.Things.Count) {
                    end = true;
                    break;
                }

                var thing = Root.Things[i];
                _container?.FakeConnect();
                if (AreaUtil.SpawnThing(thing, Root, Target, Offset) == null) {
                    FailCounter++;
                }
                _container?.FakeDisconnect();

                Spawned++;
            }

            Find.PlaySettings.autoHomeArea = autoHomeArea;
            if (end) {
                HumanStoryteller.StoryComponent.StoryStatus.CreatingStructure = false;
                Target.regionAndRoomUpdater.Enabled = true;
                Target.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
                AreaUtil.FloodStructureZone(Target, Root, Offset);
                Tell.Log("Spawned structure. Complete size was " + Root.Things.Count + " of with " + FailCounter +
                         " things failed to spawn.");
                Ir.CreatedStructure();
            } else {
                HumanStoryteller.StoryComponent.StoryQueue.Add(this);
            }
        }
        
        public void ExposeData() {
            Scribe_Values.Look(ref FailCounter, "failCounter");
            Scribe_Values.Look(ref Spawned, "spawned");
            Scribe_Deep.Look(ref Root, "root");
            Scribe_References.Look(ref Target, "target");
            Scribe_Values.Look(ref Offset, "offset");
            Scribe_References.Look(ref Ir, "ir");
        }

        public override string ToString() {
            return $"FailCounter: {FailCounter}, Spawned: {Spawned}, Root: {Root}, Target: {Target}, Offset: {Offset}, Ir: {Ir}";
        }
    }
}