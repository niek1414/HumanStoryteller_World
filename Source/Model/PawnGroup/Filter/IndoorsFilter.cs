using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class IndoorsFilter : PawnGroupFilter {
        public const String Name = "Indoors";

        public IndoorsFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return !p.Position.UsesOutdoorTemperature(p.Map);
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}