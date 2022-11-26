using HumanStoryteller.Incidents;
using HumanStoryteller.Model.Incident;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.StoryPart; 
public class HumanIncidentParams : IExposable {
    public Target Target;
    public HumanLetter Letter;

    public HumanIncidentParams() {
        Target = new Target();
        Letter = null;
    }

    public HumanIncidentParams(Target target, HumanLetter letter) {
        Target = target;
        Letter = letter;
    }

    public override string ToString() {
        return $"T: [{Target}]";
    }

    public IIncidentTarget GetTarget() {
        return Target.GetMapFromTarget();
    }

    public virtual void ExposeData() {
        Scribe_Deep.Look(ref Target, "target");
        Scribe_Deep.Look(ref Letter, "letter");
    }
}