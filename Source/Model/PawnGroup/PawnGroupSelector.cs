using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HumanStoryteller.Model.PawnGroup {
    public class PawnGroupSelector : IExposable {
        public PawnGroupSource Source = new PawnGroupSource();
        public List<PawnGroupFilter> Filters = new List<PawnGroupFilter>();
        public Number Limit = new Number();
        
        private readonly Random _r = new Random();
        
        public PawnGroupSelector() {
        }

        public List<Pawn> Filter(Map map) {
            var pawns = Source.GetSource(map).Pawns
                .Where(pawn => pawn.Spawned && !pawn.Discarded && pawn.Destroyed);
            foreach (var filter in Filters) {
                pawns = pawns.Where(pawn => filter?.ExecuteFilter(pawn, map) ?? true);
            }

            var limit = (int) Limit.GetValue();
            return pawns.OrderBy(x => _r.NextDouble()).Take(limit == -1 ? int.MaxValue : limit).ToList();
        }

        public override string ToString() {
            return $"Source: {Source}, Filters: {Filters}, Limit: {Limit}";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref Source, "source");
            Scribe_Collections.Look(ref Filters, "filters", LookMode.Deep);
            Scribe_Deep.Look(ref Limit, "limit");
        }
    }
}