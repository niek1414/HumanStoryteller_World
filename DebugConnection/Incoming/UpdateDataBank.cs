using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.DebugConnection.Incoming; 
public class UpdateDataBank : IncomingMessage {
    public UpdateDataBank() : base(MessageType.UpdateDataBank) {
    }

    public override string ToString() {
        return $"Name: [{nameof(UpdateDataBank)}]";
    }

    public override void Handle() {
        Tell.Log("Handling incoming message: " + ToString());
        if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.IsNoStory) {
            Tell.Warn("Ignoring..");
            return;
        }

        DebugWebSocket.TryUpdateDataBanks();
    }
}