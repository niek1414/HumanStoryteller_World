using System;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_OnHit : HumanIncidentWorker {
        public const String Name = "OnHit";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_OnHit)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_OnHit allParams =
                Tell.AssertNotNull((HumanIncidentParams_OnHit) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            var qir = new IncidentResult_QueueEvent();
            ShotReportUtil.SaveShotReport(allParams.HitResponse, allParams.SendingPawns, allParams.ReceivingPawns, qir);
            
            SendLetter(allParams);
            return qir;
        }
    }

    public class HumanIncidentParams_OnHit : HumanIncidentParms {
        public PawnGroupSelector ReceivingPawns = new PawnGroupSelector();
        public PawnGroupSelector SendingPawns = new PawnGroupSelector();
        public ShotReportUtil.HitResponseType HitResponse = ShotReportUtil.HitResponseType.AlwaysHit;

        public HumanIncidentParams_OnHit() {
        }

        public HumanIncidentParams_OnHit(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, ReceivingPawns: [{ReceivingPawns}], SendingPawns: [{SendingPawns}], HitResponse: [{HitResponse}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref ReceivingPawns, "receivingPawns");
            Scribe_Deep.Look(ref SendingPawns, "sendingPawns");
            Scribe_Values.Look(ref HitResponse, "hitResponse");
        }
    }
}