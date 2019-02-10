using HumanStoryteller.Incidents;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public abstract class CheckCondition : IExposable {
        public abstract bool Check(IncidentResult result);
        public virtual void ExposeData() {}
    }
}