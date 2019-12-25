using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class CaravanLocationCheck : CheckCondition {
        public const String Name = "CaravanLocation";

        public CaravanLocationCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            List<Caravan> caravans = Find.WorldObjects.Caravans;
            return Enumerable.Any(caravans, t => t.IsPlayerControlled && t.Tile == result.Target.GetTileFromTarget());
        }

        public override string ToString() {
            return "CaravanLocationCheck";
        }

        public override void ExposeData() {
            base.ExposeData();
        }
    }
}