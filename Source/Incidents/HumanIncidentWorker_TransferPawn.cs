using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TransferPawn : HumanIncidentWorker {
        public const String Name = "TransferPawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_TransferPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TransferPawn allParams = Tell.AssertNotNull((HumanIncidentParams_TransferPawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            allParams.Pawns.Filter(map);
            
            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_TransferPawn : HumanIncidentParms {
        public PawnGroupSelector Pawns;

        public HumanIncidentParams_TransferPawn() {
        }

        public HumanIncidentParams_TransferPawn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Pawns: [{Pawns}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
        }
    }
}