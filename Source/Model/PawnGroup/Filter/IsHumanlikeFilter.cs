using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class IsHumanlikeFilter : PawnGroupFilter {
        public const String Name = "IsHumanlike";

        public IsHumanlikeFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.kindDef.RaceProps.Humanlike;
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}