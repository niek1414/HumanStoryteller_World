using System;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class AwakeFilter : PawnGroupFilter {
        public const String Name = "Awake";

        public AwakeFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.Awake();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}