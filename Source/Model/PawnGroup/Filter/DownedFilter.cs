using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class DownedFilter : PawnGroupFilter {
        public const String Name = "Downed";

        public DownedFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.Downed;
        }

        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}