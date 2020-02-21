using System.Threading;
using HumanStoryteller.Util.Logging;
using Verse;
using Story = HumanStoryteller.Model.StoryPart.Story;

namespace HumanStoryteller.DebugConnection.Incoming {
    public class LoadStory : IncomingMessage {
        public bool Reboot;
        public string Story;

        public LoadStory() : base(MessageType.LoadStory) {
        }

        public override string ToString() {
            return $"Name: [{nameof(LoadStory)}] Reboot: [{Reboot}], Story: [{Story}]";
        }

        public override void Handle() {
            Tell.Log("Handling incoming message: " + ToString());
            var story = Parser.Parser.StoryParser(Story);
            if (Reboot) {
                UnloadAndReloadWithLocalStory("G", story);
            } else {
                if (!GenScene.InPlayScene) {
                    UnloadAndReloadWithLocalStory("W", story);
                } else {
                    HumanStoryteller.GetStoryCallback(story);
                }
            }
        }

        private static void UnloadAndReloadWithLocalStory(string identifier, Story story) {
            GenScene.GoToMainMenu();
            LongEventHandler.QueueLongEvent(() =>
            {
                var newThread = new Thread(delegate() {
                    Thread.Sleep(500);
                    StorytellerCompProperties_HumanThreatCycle.StartHumanStorytellerGame("-1", identifier, story);
                });
                newThread.Start();
            }, "LoadingLongEvent", true, null);
        }
    }
}