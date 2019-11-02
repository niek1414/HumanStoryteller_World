using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PointTo : HumanIncidentWorker {
        public const String Name = "PointTo";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_PointTo)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PointTo
                allParams = Tell.AssertNotNull((HumanIncidentParams_PointTo) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            var sc = HumanStoryteller.StoryComponent;
            if (!allParams.RemoveAll) {
                sc.StoryOverlay.AddItem(new Pointer(map, allParams.Location.GetSingleCell(map)));
            } else {
                sc.StoryOverlay.NotifyEnd<Pointer>();
            }

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_PointTo : HumanIncidentParms {
        public bool RemoveAll;
        public Location Location = new Location();

        public HumanIncidentParams_PointTo() {
        }

        public HumanIncidentParams_PointTo(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, RemoveAll: [{RemoveAll}], Location: [{Location}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref RemoveAll, "removeAll");
            Scribe_Deep.Look(ref Location, "location");
        }
    }
}