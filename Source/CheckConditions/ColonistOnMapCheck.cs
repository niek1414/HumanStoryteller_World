using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class ColonistOnMapCheck : CheckCondition {
        public const String Name = "ColonistOnMap";

        private string _mapName;
        private string _pawnName;

        public ColonistOnMapCheck() {
        }

        public ColonistOnMapCheck(string mapName, string pawnName) {
            _mapName = Tell.AssertNotNull(mapName, nameof(mapName), GetType().Name);
            _pawnName = Tell.AssertNotNull(pawnName, nameof(pawnName), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            var map = MapUtil.GetMapByName(_mapName, false);
            if (map == null) return false;
            var pawn = PawnUtil.GetPawnByName(_pawnName);
            if (pawn.DestroyedOrNull()) return false;
            return map.mapPawns.AllPawns.Contains(pawn);
        }

        public override string ToString() {
            return $"MapName: [{_mapName}], PawnName: [{_pawnName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _mapName, "mapName");
            Scribe_Values.Look(ref _pawnName, "pawnName");
        }
    }
}