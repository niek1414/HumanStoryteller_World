using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using HarmonyLib;
using HumanStoryteller.DebugConnection;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Web;
using RimWorld;
using Verse;
using Timer = System.Timers.Timer;

namespace HumanStoryteller; 
public class StorytellerComp_HumanThreatCycle : StorytellerComp {
    private HumanStoryteller.RefreshRate _currentRate = HumanStoryteller.RefreshRate.Long;
    private static readonly MainButtonDef RateButtonDef = DefDatabase<MainButtonDef>.AllDefs.First(x => x.defName == "RateTab");
    public static int IP => Find.TickManager.TicksGame / 60; // 1/100 of a day

    protected StorytellerCompProperties_HumanThreatCycle Props =>
        (StorytellerCompProperties_HumanThreatCycle) props;


    private bool _init;
    public readonly Timer RefreshTimer = new Timer();
    public readonly Timer ActionTimer = new Timer();

    public static void Tick() {
        HumanStoryteller.HumanStorytellerGame = false;
        if (!DebugSettings.enableStoryteller) return;
        foreach (var comp in Current.Game.storyteller.storytellerComps) {
            if (comp.GetType() == typeof(StorytellerComp_HumanThreatCycle) && Find.TickManager.TicksGame > 0) {
                ((StorytellerComp_HumanThreatCycle) comp).CycleTick();
                HumanStoryteller.HumanStorytellerGame = true;
            }
        }

        if (Find.TickManager.TicksGame % 50 != 0) return;
        RateButtonDef.buttonVisible = !HumanStoryteller.IsNoStory;
    }

    public void CycleTick() {
        if (!_init) {
            Init();
            _init = true;
        } else if (HumanStoryteller.StoryComponent.ForcedUpdate) {
            HumanStoryteller.StoryComponent.Reset();
            Init();
        }

        if (HumanStoryteller.StoryComponent.StoryArc == null) return;

        StoryQueueTick();
        HumanStoryteller.StoryComponent.StoryArc.LongStoryController.Tick();
        HumanStoryteller.StoryComponent.StoryArc.ShortStoryController.Tick();
    }


    private void CheckStoryRefresh(object source, ElapsedEventArgs e) {
        if (Current.Game == null || HumanStoryteller.StoryComponent == null || !(Find.TickManager.TicksGame > 0)) {
            RefreshTimer.Enabled = false;
            Tell.Warn("Tried to get story while not in-game");
            return;
        }

        Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId, story => HumanStoryteller.GetStoryCallback(story, this));
    }

    private void Init() {
        if (!HumanStoryteller.StoryComponent.Initialised) {
            HumanStoryteller.StoryComponent.Initialised = true;
            Tell.Log("STORYTELLER AWOKEN", HumanStoryteller.StoryComponent.StoryId);
            HumanStoryteller.StoryComponent.FirstMapOfPlayer = Find.Maps.Find(x => x.ParentFaction.IsPlayer);
            HumanStoryteller.StoryComponent.SameAsLastEvent = HumanStoryteller.StoryComponent.FirstMapOfPlayer;
            HumanStoryteller.StoryComponent.LastColonizedMap = HumanStoryteller.StoryComponent.FirstMapOfPlayer;
            Tell.Log("RECORDED HISTORY");
        } else {
            Tell.Log("CONTINUING HS GAME", HumanStoryteller.StoryComponent.StoryId);
            Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId, story => HumanStoryteller.GetStoryCallback(story, this));
        }

        HumanStoryteller.StoryComponent.ThreatCycle = this;

        Messages.Message(
            $"HumanStoryteller {"Story".Translate()}: #{HumanStoryteller.StoryComponent.StoryId}, {"ALPHA_BUILD".Translate()}: {HumanStoryteller.VERSION_NAME} ({HumanStoryteller.VERSION})",
            MessageTypeDefOf.PositiveEvent);

        if (HumanStoryteller.IsNoStory) {
            Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId, story => HumanStoryteller.GetStoryCallback(story, this));
        }

        RefreshTimer.Elapsed += CheckStoryRefresh;
        RefreshTimer.Interval = HumanStoryteller.LONG_REFRESH;
        RefreshTimer.Enabled = true;

        // ActionTimer.Elapsed += (sender, args) => { StoryQueueTick(); };
        // ActionTimer.Interval = 100;
        // ActionTimer.Enabled = true;

        //WRITE DEFS
        // FileLog.Log(JsonConvert.SerializeObject(ExtractDefs.ExtractCurrentDefs(), Formatting.None, new JsonSerializerSettings {
        //     NullValueHandling = NullValueHandling.Ignore,
        //     Converters = new List<JsonConverter> {new StringEnumConverter()}
        // }));
    }

    private static void StoryQueueTick() {
        try {
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.StoryComponent.StoryArc == null ||
                !HumanStoryteller.StoryComponent.Initialised) return;
            HumanStoryteller.StoryComponent.StoryQueue.Tick();
        } catch (Exception e) {
            Tell.Err("Error while ticking story queue, " + e.Message, e);
        }
    }

    public HumanStoryteller.RefreshRate CurrentRate => _currentRate;

    public void SetRefreshRate(HumanStoryteller.RefreshRate rate) {
        switch (rate) {
            case HumanStoryteller.RefreshRate.Short:
                RefreshTimer.Interval = HumanStoryteller.SHORT_REFRESH;
                _currentRate = HumanStoryteller.RefreshRate.Short;
                break;
            case HumanStoryteller.RefreshRate.Medium:
                RefreshTimer.Interval = HumanStoryteller.MEDIUM_REFRESH;
                _currentRate = HumanStoryteller.RefreshRate.Medium;
                break;
            case HumanStoryteller.RefreshRate.Long:
                RefreshTimer.Interval = HumanStoryteller.LONG_REFRESH;
                _currentRate = HumanStoryteller.RefreshRate.Long;
                break;
            case HumanStoryteller.RefreshRate.Off:
                RefreshTimer.Interval = HumanStoryteller.OFF_REFRESH;
                _currentRate = HumanStoryteller.RefreshRate.Off;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rate), rate, null);
        }
    }
}