using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using HumanStoryteller.Util.Logging;
using RimWorld;
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
            int initCount = 0;
            var pawns = Source.GetSource(map).Pawns
                .Where(pawn => {
                    var r = !pawn.DestroyedOrNull() && !pawn.Discarded;
                    if (r) {
                        initCount++;
                    }
                    return r;
                });
            foreach (var filter in Filters) {
                pawns = pawns.Where(pawn => filter?.ExecuteFilter(pawn, map) ?? true);
            }

            var limit = (int) Limit.GetValue();
            var realLimit = limit == -1 ? int.MaxValue : limit;
            var result = pawns.OrderBy(x => _r.NextDouble()).Take(realLimit).ToList();
            Tell.Log("Filtered group selection, source: " + initCount + " after filter: " + result.Count + " limited if above: " + realLimit);
            if (HumanStoryteller.DEBUG) {
                foreach (var pawn in result) {
                    Tell.Debug("Found pawn: " + pawn.Name + " _ " + pawn.GetInspectString());
                }
            }
            return result;
        }

        public override string ToString() {
            return $"Source: [{Source}], Filters: [{Filters.Join(f => f.GetType() + ": " + f.ToString())}], Limit: [{Limit}]";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref Source, "source");
            Scribe_Collections.Look(ref Filters, "filters", LookMode.Deep);
            Scribe_Deep.Look(ref Limit, "limit");
        }
    }
}