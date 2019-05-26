using System;
using System.Threading;
using System.Timers;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using Verse;

namespace HumanStoryteller {
    public class HumanStoryteller : Mod {
        public static string VERSION = "0.2.2";
        public static string VERSION_NAME = "`Controller`";

        public static bool InitiateEventUnsafe = false;

        public const int SHORT_REFRESH = 60000 * 2;
        public const int MEDIUM_REFRESH = 60000 * 10;
        public const int LONG_REFRESH = 60000 * 60;
        public const int OFF_REFRESH = Int32.MaxValue;

        public enum RefreshRate {
            Short,
            Medium,
            Long,
            Off
        }

        public static StoryComponent StoryComponent =>
            Tell.AssertNotNull(Current.Game?.GetComponent<StoryComponent>(), nameof(StoryComponent), "HumanStoryteller");

        public static bool HumanStorytellerGame;
        public static bool IsNoStory => StoryComponent.Story == null;

        public static long StoryId =>
            IsNoStory ? -1 : Tell.AssertNotNull(StoryComponent.Story.Id, nameof(StoryComponent.Story.Id), "HumanStoryteller");

        public static bool DEBUG => true;

        public HumanStoryteller(ModContentPack content) : base(content) {
//            Thread humanStoryThread = new Thread(SwitchStoryteller);
//            humanStoryThread.Start();
        }

        private void SwitchStoryteller() {
//            while (true) {
//                if (Current.Game != null) {
//                    Tell.Log(Current.Game.DebugString());
//                }
//            }
        }

        public static void GetStoryCallback(Story story, StorytellerComp_HumanThreatCycle cycle = null) {
            if (cycle != null && (Current.Game == null || StoryComponent == null || !(Find.TickManager.TicksGame > 0))) {
                cycle.RefreshTimer.Enabled = false;
                Tell.Warn("Tried to get story while not in-game");
                return;
            }

            if (story == null) {
                if (cycle != null) {
                    cycle.RefreshTimer.Enabled = false;
                }
                Messages.Message(Translator.Translate("StoryNotFound"), MessageTypeDefOf.NegativeEvent, false);
                return;
            }
            var sc = StoryComponent;
            InitiateEventUnsafe = true;
            Thread.Sleep(1000); //Give some time to finish undergoing event executions
            sc.Story = story;
            sc.AllNodes = sc.Story.StoryGraph.GetAllNodes();
            if (sc.CurrentNodes.Count == 0) {
                sc.CurrentNodes.Add(new StoryEventNode(sc.Story.StoryGraph.Root, Find.TickManager.TicksGame / 600));
            } else {
                for (int i = 0; i < sc.CurrentNodes.Count; i++) {
                    var foundNode =
                        sc.Story.StoryGraph.GetCurrentNode(sc.CurrentNodes[i]?.StoryNode
                            .StoryEvent.Uuid);
                    sc.CurrentNodes[i] = foundNode == null
                        ? null
                        : new StoryEventNode(foundNode, sc.CurrentNodes[i].ExecuteTick, sc.CurrentNodes[i].Result);
                }

                sc.CurrentNodes.RemoveAll(item => item == null);
            }

            InitiateEventUnsafe = false;
        }
    }


    [StaticConstructorOnStartup]
    static class HarmonyPatches {
        static HarmonyPatches() {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.keyboxsoftware.humanstoryteller");

            Patch.Main_Patch.Patch(harmony);
            Patch.StorytellerUI_Patch.Patch(harmony);
            Patch.CreateWorldUI_Patch.Patch(harmony);
            Patch.SelectStartingSiteUI_Patch.Patch(harmony);
            Patch.Lord_Patch.Patch(harmony);
            Patch.Map_Patch.Patch(harmony);
        }
    }
}