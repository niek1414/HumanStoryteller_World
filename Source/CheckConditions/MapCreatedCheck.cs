using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class MapCreatedCheck : CheckCondition {
        public const String Name = "MapCreated";

        private String _mapName;

        public MapCreatedCheck() {
        }

        public MapCreatedCheck(string mapName) {
            _mapName = Tell.AssertNotNull(mapName, nameof(mapName), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Map map = MapUtil.GetMapByName(_mapName, false);
            return map != null;
        }

        public override string ToString() {
            return $"MapName: [{_mapName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _mapName, "mapName");
        }
    }
}