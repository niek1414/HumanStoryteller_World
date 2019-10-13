using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class PrisonerInCellFilter : PawnGroupFilter {
        public const String Name = "PrisonerInCell";

        public PrisonerInCellFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.IsPrisonerInPrisonCell();
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}