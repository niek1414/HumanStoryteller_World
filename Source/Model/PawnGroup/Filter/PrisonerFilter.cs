using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class PrisonerFilter : PawnGroupFilter {
        public const String Name = "Prisoner";

        public PrisonerFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.IsPrisoner;
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}