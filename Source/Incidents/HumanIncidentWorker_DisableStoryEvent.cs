using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_DisableStoryEvent : HumanIncidentWorker {
        public const String Name = "DisableStoryEvent";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_DisableStoryEvent)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_DisableStoryEvent
                allParams = Tell.AssertNotNull((HumanIncidentParams_DisableStoryEvent) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var sc = HumanStoryteller.StoryComponent;
            sc.StoryStatus.DisableGameOverDialog = allParams.DisableGameOverDialog;
            sc.StoryStatus.DisableNameColonyDialog = allParams.DisableNameColonyDialog;

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_DisableStoryEvent : HumanIncidentParms {
        public bool DisableGameOverDialog;
        public bool DisableNameColonyDialog;

        public HumanIncidentParams_DisableStoryEvent() {
        }

        public HumanIncidentParams_DisableStoryEvent(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, DisableGameOverDialog: [{DisableGameOverDialog}], DisableNameColonyDialog: [{DisableNameColonyDialog}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref DisableGameOverDialog, "disableGameOverDialog");
            Scribe_Values.Look(ref DisableNameColonyDialog, "disableNameColonyDialog");
        }
    }
}