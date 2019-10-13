using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class StarvingFilter : PawnGroupFilter {
        public const String Name = "Starving";

        public StarvingFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.Starving();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}