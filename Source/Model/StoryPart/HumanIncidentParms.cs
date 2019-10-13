using HumanStoryteller.Incidents;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.StoryPart {
    public class HumanIncidentParms : IExposable {
        public Target Target;
        public HumanLetter Letter;

        public HumanIncidentParms() {
            Target = new Target();
            Letter = null;
        }

        public HumanIncidentParms(Target target, HumanLetter letter) {
            Target = target;
            Letter = letter;
        }

        public override string ToString() {
            return $"T: {Target}";
        }

        public IIncidentTarget GetTarget() {
            return Target.GetMapFromTarget();
        }

        public virtual void ExposeData() {
            Scribe_Deep.Look(ref Target, "target");
            Scribe_Deep.Look(ref Letter, "letter");
        }
    }
}