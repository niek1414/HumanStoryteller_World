using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RadioMessage : HumanIncidentWorker {
        public const String Name = "RadioMessage";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_RadioMessage)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RadioMessage
                allParams = Tell.AssertNotNull((HumanIncidentParams_RadioMessage) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var message = new RadioMessage(PawnUtil.GetPawnByName(allParams.Name), allParams.Message.Get());
            HumanStoryteller.StoryComponent?.StoryOverlay.AddRadio(message);
            
            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_RadioMessage : HumanIncidentParms {
        public string Name = "";
        public RichText Message = new RichText();

        public HumanIncidentParams_RadioMessage() {
        }

        public HumanIncidentParams_RadioMessage(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: [{Name}], Message: [{Message}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Name, "name");
            Scribe_Deep.Look(ref Message, "message");
        }
    }
}