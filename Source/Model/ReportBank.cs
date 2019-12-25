using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Model {
    public class ReportBank : IExposable {
        public ShotReportUtil.HitResponseType Type;
        public PawnGroupSelector Senders;
        public PawnGroupSelector Receivers;
        public IncidentResult_QueueEvent Ir;

        public ReportBank() {
        }

        public ReportBank(ShotReportUtil.HitResponseType report, PawnGroupSelector senders, PawnGroupSelector receivers,
            IncidentResult_QueueEvent ir) {
            Type = report;
            Senders = senders;
            Receivers = receivers;
            Ir = ir;
        }

        public bool CheckMatch(Pawn sender, Pawn receiver) {
            return Senders.FilterEnumerable(Find.CurrentMap).Contains(sender) && Receivers.FilterEnumerable(Find.CurrentMap).Contains(receiver);
        }

        public override string ToString() {
            return $"ShotReport: [ Type: [{Type}],Senders: [{Senders}], Receivers: [{Receivers}]]";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref Type, "type");
            Scribe_Deep.Look(ref Senders, "senders");
            Scribe_Deep.Look(ref Receivers, "receivers");
            Scribe_References.Look(ref Ir, "ir");
        }
    }
}