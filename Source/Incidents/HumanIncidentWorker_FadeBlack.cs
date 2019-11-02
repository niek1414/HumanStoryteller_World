using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_FadeBlack : HumanIncidentWorker {
        public const String Name = "FadeBlack";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_FadeBlack)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_FadeBlack
                allParams = Tell.AssertNotNull((HumanIncidentParams_FadeBlack) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var sc = HumanStoryteller.StoryComponent;
            if (allParams.Enable) {
                sc.StoryOverlay.AddItem(new FadeBlack());
            } else {
                sc.StoryOverlay.NotifyEnd<FadeBlack>();
            }

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_FadeBlack : HumanIncidentParms {
        public bool Enable;

        public HumanIncidentParams_FadeBlack() {
        }

        public HumanIncidentParams_FadeBlack(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Enable: [{Enable}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Enable, "enable");
        }
    }
}