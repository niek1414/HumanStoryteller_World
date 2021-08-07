using System;
using Verse;
using Verse.AI;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class CanSeeOneOfFilter : PawnGroupFilter {
        public const String Name = "CanSeeOneOf";

        public PawnGroupSource Target;
        
        public CanSeeOneOfFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return Target.GetSource(null).Pawns.Any(target => p.CanSee(target));
        }
        
        protected override void ExposeFilter() {
            Scribe_Deep.Look(ref Target, "target");
        }

        public override string ToString() {
            return $"{base.ToString()}, Target: [{Target}]";
        }
    }
}