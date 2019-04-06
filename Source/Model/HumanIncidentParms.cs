using System;
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
            return "";
        }
        
        public IIncidentTarget GetTarget() {
            switch (Target) {
                case "OfPlayer":
                    return Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
                default:
                    return Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
            }
        }
        
        public virtual void ExposeData() {
            Scribe_Values.Look(ref Target, "target");
            Scribe_Deep.Look(ref Letter, "leter");
        }
    }
}