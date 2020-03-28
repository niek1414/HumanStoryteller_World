using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class MaleFilter : PawnGroupFilter {
        public const String Name = "Male";

        public MaleFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            return p.gender == Gender.Male;
        }
        
        protected override void ExposeFilter() {
        }

        public override string ToString() {
            return $"{base.ToString()}";
        }
    }
}