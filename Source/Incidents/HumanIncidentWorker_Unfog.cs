using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Unfog : HumanIncidentWorker {
        public const String Name = "Unfog";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Unfog)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Unfog
                allParams = Tell.AssertNotNull((HumanIncidentParams_Unfog) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            CellIndices cellIndices = map.cellIndices;
            if (map.fogGrid.fogGrid == null) {
                map.fogGrid.fogGrid = new bool[cellIndices.NumGridCells];
            }

            for (var i = 0; i < map.fogGrid.fogGrid.Length; i++) {
                map.fogGrid.fogGrid[i] = true;
            }

            if (allParams.Unfog) {
                var cell = allParams.Location.GetSingleCell(map, false);
                if (cell.IsValid) {
                    FloodFillerFog.FloodUnfog(cell, map);
                } else {
                    foreach (var pawn in map.mapPawns.FreeColonistsSpawned) {
                        FloodFillerFog.FloodUnfog(pawn.Position, map);
                    }
                }
            }
            foreach (IntVec3 allCell in map.AllCells) {
                map.mapDrawer.MapMeshDirty(allCell, MapMeshFlag.FogOfWar);
            }

            if (Current.ProgramState == ProgramState.Playing) {
                map.roofGrid.Drawer.SetDirty();
            }

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_Unfog : HumanIncidentParms {
        public bool Unfog;
        public Location Location;

        public HumanIncidentParams_Unfog() {
        }

        public HumanIncidentParams_Unfog(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Unfog: [{Unfog}], Location: [{Location}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Unfog, "unfog");
            Scribe_Deep.Look(ref Location, "location");
        }
    }
}