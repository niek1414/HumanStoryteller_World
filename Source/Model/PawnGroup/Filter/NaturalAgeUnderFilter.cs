using System;
using HumanStoryteller.Model.Incident;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class NaturalAgeUnderFilter : PawnGroupFilter {
        public const String Name = "NaturalAgeUnder";

        public Number Age = new Number();

        public NaturalAgeUnderFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.ageTracker.AgeBiologicalYears < Age.GetValue();
        }
        
        protected override void ExposeFilter() {
            Scribe_Deep.Look(ref Age, "age");
        }

        public override string ToString() {
            return $"{base.ToString()}, Age: [{Age}]";
        }
    }
}