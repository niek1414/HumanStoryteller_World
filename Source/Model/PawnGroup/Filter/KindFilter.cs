using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class KindFilter : PawnGroupFilter {
        public const String Name = "Kind";

        public List<String> Kinds = new List<String>();
        private List<PawnKindDef> _temp;

        public KindFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            if (_temp == null) {
                _temp = Kinds.ConvertAll(PawnKindDef.Named);
            }
            return _temp.Contains(p.kindDef);
        }
        
        protected override void ExposeFilter() {
            Scribe_Collections.Look(ref Kinds, "kinds", LookMode.Value);
        }

        public override string ToString() {
            return $"{base.ToString()}, Kinds: [{Kinds.Join()}]";
        }
    }
}