using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SavePawnGroup : HumanIncidentWorker {
        public const String Name = "SavePawnGroup";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_SavePawnGroup)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_SavePawnGroup allParams =
                Tell.AssertNotNull((HumanIncidentParams_SavePawnGroup) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            PawnGroupUtil.SaveGroupByName(allParams.OutGroup, new PawnGroup(allParams.Pawns.Filter(map)));
            
            SendLetter(parms);
            
            return ir;
        }
    }

    public class HumanIncidentParams_SavePawnGroup : HumanIncidentParms {
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string OutGroup;

        public HumanIncidentParams_SavePawnGroup() {
        }

        public HumanIncidentParams_SavePawnGroup(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "pawns");
            Scribe_Values.Look(ref OutGroup, "outGroup");
        }

        public override string ToString() {
            return $"{base.ToString()}, Pawns: [{Pawns}], OutGroup: [{OutGroup}]";
        }
    }
}