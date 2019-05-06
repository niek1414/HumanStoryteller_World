using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class PawnLocationCheck : CheckCondition {
        public const String Name = "PawnLocation";
        private string _pawnName;
        private string _location;
        private float _radius;

        public PawnLocationCheck() {
        }

        public PawnLocationCheck(string pawnName, string location, float chance) {
            _pawnName = Tell.AssertNotNull(pawnName, nameof(pawnName), GetType().Name);
            _location = Tell.AssertNotNull(location, nameof(location), GetType().Name);
            _radius = Tell.AssertNotNull(chance, nameof(chance), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Pawn p = PawnUtil.GetPawnByName(_pawnName);
            var target = result.GetTarget();
            if (p == null || p.Map != target) {
                return false;
            }

            IntVec3 loc = MapUtil.FindLocationByName(_location, target);
            return loc.InHorDistOf(p.Position, _radius);
        }

        public override string ToString() {
            return $"PawnName: {_pawnName}, Location: {_location}, Radius: {_radius}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _pawnName, "pawnName");
            Scribe_Values.Look(ref _location, "location");
            Scribe_Values.Look(ref _radius, "radius");
        }
    }
}