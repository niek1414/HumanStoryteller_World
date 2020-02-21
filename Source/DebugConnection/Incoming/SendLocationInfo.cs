using HumanStoryteller.Util.Logging;
using HumanStoryteller.Views;
using Verse;

namespace HumanStoryteller.DebugConnection.Incoming {
    public class SendLocationInfo : IncomingMessage {
        public bool IsZone;

        public SendLocationInfo() : base(MessageType.SendLocationInfo) {
        }

        public override string ToString() {
            return $"Name: [{nameof(SendLocationInfo)}], IsZone: [{IsZone}]";
        }

        public override void Handle() {
            Tell.Log("Handling incoming message: " + ToString());
            if (Current.Game == null) {
                Tell.Warn("Ignoring..");
                return;
            }

            Find.WindowStack.Add(new Window_CopyZone(true, !IsZone));
        }
    }
}