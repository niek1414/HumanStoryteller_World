using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Util {
    public class ShotReportUtil {
        public static bool GetShotReport(Pawn sending, Pawn receiving, out HitResponseType? type) {
            var reportBank = HumanStoryteller.StoryComponent.ShotReportBank;
            type = null;
            ReportBank reportBankItem = null;
            foreach (var r in reportBank) {
                if (r.CheckMatch(sending, receiving)) {
                    Tell.Log("Found shot report for sending: " + sending.ToStringSafe() + " and receiving: " + receiving.ToStringSafe());
                    reportBankItem = r;
                    break;
                }
            }

            if (reportBankItem != null) {
                type = reportBankItem.Type;
                reportBankItem.Ir.QueueEventFired();
                reportBank.Remove(reportBankItem);
                return true;
            }

            return false;
        }

        public static void SaveShotReport(HitResponseType type, PawnGroupSelector senders, PawnGroupSelector receivers, IncidentResult_QueueEvent qir) {
            HumanStoryteller.StoryComponent.ShotReportBank.Add(new ReportBank(type, senders, receivers, qir));
        }

        public enum HitResponseType {
            AlwaysHit,
            AlwaysMis,
            Unaltered
        }
    }
}