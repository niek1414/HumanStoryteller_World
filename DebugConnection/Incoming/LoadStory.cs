using System.Threading;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.DebugConnection.Incoming; 
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
        var storyArc = Parser.Parser.StoryParser(Story);
        if (Reboot) {
            UnloadAndReloadWithLocalStory("G", storyArc);
        } else {
            if (!GenScene.InPlayScene) {
                UnloadAndReloadWithLocalStory("W", storyArc);
            } else {
                HumanStoryteller.GetStoryCallback(storyArc);
            }
        }
    }

    private static void UnloadAndReloadWithLocalStory(string identifier, StoryArc storyArc) {
        GenScene.GoToMainMenu();
        LongEventHandler.QueueLongEvent(() =>
        {
            var newThread = new Thread(delegate() {
                Thread.Sleep(500);
                StorytellerCompProperties_HumanThreatCycle.StartHumanStorytellerGame("-1", identifier, storyArc);
            });
            newThread.Start();
        }, "LoadingLongEvent", true, null);
    }
}