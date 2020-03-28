using System;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class IsKidnappedFilter : PawnGroupFilter {
        public const String Name = "IsKidnapped";

        public IsKidnappedFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.IsKidnapped();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}