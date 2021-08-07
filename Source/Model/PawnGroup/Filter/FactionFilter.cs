using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter {
    public class FactionFilter : PawnGroupFilter {
        public const String Name = "Faction";

        public List<String> Factions = new List<String>();
        private List<Faction> _temp;

        public FactionFilter() {
        }

        protected override bool Filter(Pawn p, Map map) {
            if (_temp == null) {
                _temp = Factions.ConvertAll(s => Find.FactionManager.AllFactions.First(f => f.def.defName == s));
            }
            return _temp.Contains(p.Faction);
        }
        
        protected override void ExposeFilter() {
            Scribe_Collections.Look(ref Factions, "factions", LookMode.Value);
        }

        public override string ToString() {
            return $"{base.ToString()}, Factions: [{Factions}]";
        }
    }
}