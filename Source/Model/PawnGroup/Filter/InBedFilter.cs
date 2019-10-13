using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class InBedFilter : PawnGroupFilter {
        public const String Name = "InBed";

        public InBedFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.InBed();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}