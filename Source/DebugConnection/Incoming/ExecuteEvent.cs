using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.DebugConnection.Incoming {
    public class ExecuteEvent : IncomingMessage {
        public bool WithRunner;
        public string Uuid;

        public ExecuteEvent() : base(MessageType.LoadStory) {
        }

        public override string ToString() {
            return $"Name: [{nameof(ExecuteEvent)}] WithRunner: [{WithRunner}], Uuid: [{Uuid}]";
        }

        public override void Handle() {
            Tell.Log("Handling incoming message: " + ToString());
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.IsNoStory) {
                Tell.Warn("Ignoring..");
                return;
            }

            HumanStoryteller.StoryComponent.StoryArc.LongStoryController.TryExecuteEvent(Uuid, WithRunner);
            HumanStoryteller.StoryComponent.StoryArc.ShortStoryController.TryExecuteEvent(Uuid, WithRunner);
        }
    }
}