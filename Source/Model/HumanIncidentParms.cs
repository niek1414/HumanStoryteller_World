using System;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model {
    public class HumanIncidentParms : IExposable {
        public String Target;
        public HumanLetter Letter;

        public HumanIncidentParms() {
        }

        public HumanIncidentParms(String target, HumanLetter letter) {
            Target = target;
            Letter = letter;
        }

        public override string ToString() {
            return $"T: {Target}";
        }

        public IIncidentTarget GetTarget() {
            return MapUtil.GetTarget(Target);
        }
        
        public virtual void ExposeData() {
            Scribe_Values.Look(ref Target, "target");
            Scribe_Deep.Look(ref Letter, "letter");
        }
    }
}