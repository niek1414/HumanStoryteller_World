using HumanStoryteller.Model;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public abstract class CheckCondition : IExposable {
        public abstract bool Check(StoryNode sn);
        public virtual void ExposeData() {}
    }
}