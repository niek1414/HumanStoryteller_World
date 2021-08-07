using System;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class OnFireFilter : PawnGroupFilter {
        public const String Name = "OnFire";

        public OnFireFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.IsBurning();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}