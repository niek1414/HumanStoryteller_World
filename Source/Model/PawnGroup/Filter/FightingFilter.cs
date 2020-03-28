using System;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class FightingFilter : PawnGroupFilter {
        public const String Name = "Fighting";

        public FightingFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.IsFighting();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}