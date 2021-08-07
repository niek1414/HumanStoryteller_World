using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_KillPawn : HumanIncidentWorker {
        public const String Name = "KillPawn";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_KillPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_KillPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_KillPawn) @params, nameof(@params), GetType().Name);
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
            
            SendLetter(@params);
            
            return ir;
        }
    }

    public class HumanIncidentParams_KillPawn : HumanIncidentParams {
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