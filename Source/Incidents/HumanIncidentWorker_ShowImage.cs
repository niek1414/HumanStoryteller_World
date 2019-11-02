using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShowImage : HumanIncidentWorker {
        public const String Name = "ShowImage";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ShowImage)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ShowImage
                allParams = Tell.AssertNotNull((HumanIncidentParams_ShowImage) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var sc = HumanStoryteller.StoryComponent;
            if (allParams.RemoveAll) {
                sc.StoryOverlay.NotifyEnd<ShowImage>();
            } else {
                sc.StoryOverlay.AddItem(new ShowImage(allParams.Url));
            }

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_ShowImage : HumanIncidentParms {
        public bool RemoveAll;
        public string Url;

        public HumanIncidentParams_ShowImage() {
        }

        public HumanIncidentParams_ShowImage(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, RemoveAll: [{RemoveAll}], Url: [{Url}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref RemoveAll, "removeAll");
            Scribe_Values.Look(ref Url, "url");
        }
    }
}