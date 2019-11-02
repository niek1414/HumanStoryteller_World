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
    class HumanIncidentWorker_KillPawn : HumanIncidentWorker {
        public const String Name = "KillPawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_KillPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_KillPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_KillPawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            foreach (var pawn in allParams.Pawns.Filter(map)) {
                if (pawn == null) {
                    continue;
                }
                if (allParams.Destroy) {
                    pawn.Destroy();
                } else {
                    pawn.Kill(new DamageInfo(DamageDefOf.ExecutionCut, 5000f));
                }
            }
            
            SendLetter(parms);
            
            return ir;
        }
    }

    public class HumanIncidentParams_KillPawn : HumanIncidentParms {
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public bool Destroy;

        public HumanIncidentParams_KillPawn() {
        }

        public HumanIncidentParams_KillPawn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref Destroy, "destroy");
        }
    }
}