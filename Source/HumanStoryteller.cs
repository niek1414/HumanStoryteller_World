using System;
using System.Net;
using System.Threading;
using Harmony;
using HumanStoryteller.DebugConnection;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;
using WebSocketSharp.Server;

namespace HumanStoryteller {
    public class HumanStoryteller : Mod {
        public static float VERSION = 0.5f;
        public static string VERSION_NAME = "`Quality of Life`";

        public static ModContentPack ContentPack;
        public static HumanStorytellerSettings Settings;

        public static bool InitiateEventUnsafe = false;
        public static int ConcurrentActions = 0;

        public static WebSocketServer DebugWebSocketConnection = null;

        private const int MINUTE = 60000;

        public const int SHORT_REFRESH = MINUTE * 2;
        public const int MEDIUM_REFRESH = MINUTE * 10;
        public const int LONG_REFRESH = MINUTE * 60;
        public const int OFF_REFRESH = Int32.MaxValue;

        public const bool DEBUG = true;

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

        public static bool CreatorTools => HumanStorytellerSettings.EnableCreatorTools;
        public static bool DidInitialParamCheck;

        public HumanStoryteller(ModContentPack content) : base(content) {
            HumanStoryteller.ContentPack = content;
            Settings = GetSettings<HumanStorytellerSettings>();
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("EnableCreatorTools".Translate(), ref HumanStorytellerSettings.EnableCreatorTools,
                "EnableCreatorToolsToolTip".Translate());
            listingStandard.End();
            CheckDebugConnectionSetting();

            base.DoSettingsWindowContents(inRect);
        }

        public static void CheckDebugConnectionSetting() {
            if (HumanStorytellerSettings.EnableCreatorTools && DebugWebSocketConnection == null) {
                Tell.Log("Starting debug connection...");
                DebugWebSocketConnection = new WebSocketServer(661);
                DebugWebSocketConnection.AddWebSocketService<DebugWebSocket>("/");
                try {
                    DebugWebSocketConnection.Start();
                    Tell.Log("Debug connection started, you can now connect with the Storymaker!");
                } catch (InvalidOperationException e) {
                    Tell.Err(e.Message);
                }
            } else if (!HumanStorytellerSettings.EnableCreatorTools && DebugWebSocketConnection != null) {
                Tell.Log("Closing debug connection...");
                try {
                    DebugWebSocketConnection.Stop();
                    DebugWebSocketConnection = null;
                    Tell.Log("Debug connection closed");
                } catch (InvalidOperationException e) {
                    Tell.Err(e.Message);
                }
            }
        }

        public override string SettingsCategory() {
            return "HumanStoryteller".Translate();
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

                Messages.Message("StoryNotFound".Translate(), MessageTypeDefOf.NegativeEvent, false);
                return;
            }
            var sc = StoryComponent;

            var beforeLoad = Time.realtimeSinceStartup;
            Tell.Log("Start preloading all nodes");
            ConcurrentActions = 0;
            var allNodes = story.StoryGraph.GetAllNodes();
            foreach (var node in allNodes) {
                node.StoryEvent.Incident.Worker.PreLoad(node.StoryEvent.Incident.Parms);
            }

            while (ConcurrentActions != 0) {
                LongEventHandler.SetCurrentEventText("StoryAssets".Translate() + "(" + ConcurrentActions + ")");
                Thread.Sleep(10);
            }

            Tell.Log("Preloading completed in " + (Time.realtimeSinceStartup - beforeLoad));
            
            InitiateEventUnsafe = true;
            Thread.Sleep(1000); //Give some time to finish undergoing event executions
            sc.Story = story;
            sc.AllNodes = allNodes;

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
            
            DebugWebSocket.TryUpdateRunners();
        }
    }

    public class HumanStorytellerSettings : ModSettings {
        public static bool EnableCreatorTools;
        public static bool HadInitialIntroduction;

        public override void ExposeData() {
            Scribe_Values.Look(ref EnableCreatorTools, "enableCreatorTools");
            Scribe_Values.Look(ref HadInitialIntroduction, "hadInitialIntroduction");
            base.ExposeData();
        }
    }


    [StaticConstructorOnStartup]
    static class HarmonyPatches {
        static HarmonyPatches() {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.keyboxsoftware.humanstoryteller");

            Patch.Main_Patch.Patch(harmony);
            Patch.StoryStatus_Patch.Patch(harmony);
            Patch.StorytellerUI_Patch.Patch(harmony);
            Patch.CreateWorldUI_Patch.Patch(harmony);
            Patch.SelectStartingSiteUI_Patch.Patch(harmony);
            Patch.Lord_Patch.Patch(harmony);
            Patch.Map_Patch.Patch(harmony);
            Patch.Shot_Patch.Patch(harmony);
        }
    }
}