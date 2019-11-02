using Verse;

namespace HumanStoryteller.Model.PawnGroup {
    public abstract class PawnGroupFilter : IExposable {
        public bool Include;

        public PawnGroupFilter() {
        }

        public bool ExecuteFilter(Pawn p, Map map) {
            var result = Filter(p, map);
            return Include ? result : !result;
        }

        protected abstract bool Filter(Pawn p, Map map);

        public override string ToString() {
            return $"Include: [{Include}]";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Include, "include");
            ExposeFilter();
        }

        protected abstract void ExposeFilter();
    }
}