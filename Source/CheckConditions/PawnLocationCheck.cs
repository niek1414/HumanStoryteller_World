using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model.Zones;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class PawnLocationCheck : CheckCondition {
        public const String Name = "PawnLocation";
        private string _pawnName;
        private Location _location;
        private float _radius;

        public PawnLocationCheck() {
        }

        public PawnLocationCheck(string pawnName, Location location, float radius) {
            _pawnName = Tell.AssertNotNull(pawnName, nameof(pawnName), GetType().Name);
            _location = Tell.AssertNotNull(location, nameof(location), GetType().Name);
            _radius = radius;
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Pawn p = PawnUtil.GetPawnByName(_pawnName);
            var target = result.Target.GetMapFromTarget();
            if (p == null || p.Map != target) {
                return false;
            }

            if (_location.isZone()) {
                return _location.GetZone(target).Contains(p.Position);
            }

            var cell = _location.GetSingleCell(target, false);
            return cell.IsValid && cell.InHorDistOf(p.Position, _radius);
        }

        public override string ToString() {
            return $"PawnName: {_pawnName}, Location: {_location}, Radius: {_radius}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _pawnName, "pawnName");
            Scribe_Deep.Look(ref _location, "location");
            Scribe_Values.Look(ref _radius, "radius");
        }
    }
}