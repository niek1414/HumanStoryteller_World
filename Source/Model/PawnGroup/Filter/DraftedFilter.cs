using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class DraftedFilter : PawnGroupFilter {
        public const String Name = "Drafted";

        public DraftedFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.Drafted;
        }

        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}