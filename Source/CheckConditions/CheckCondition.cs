using Verse;

namespace HumanStoryteller.CheckConditions {
    public abstract class CheckCondition : IExposable {
        public abstract bool Check();
        public virtual void ExposeData() {}
    }
}