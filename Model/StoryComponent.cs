using System;
using System.Collections.Generic;
using HarmonyLib;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util; 
public class StoryComponent : GameComponent {
    public bool Initialised;
    public StoryArc StoryArc;
    public long StoryId;
    public bool ForcedUpdate;
    public List<Map> PersistentMaps = new List<Map>();
    public List<ReportBank> ShotReportBank = new List<ReportBank>();
    public Dictionary<string, float> VariableBank = new Dictionary<string, float>();
    public Dictionary<string, Pawn> PawnBank = new Dictionary<string, Pawn>();
    public Dictionary<string, PawnGroup> PawnGroupBank = new Dictionary<string, PawnGroup>();
    public Dictionary<string, MapContainer> MapBank = new Dictionary<string, MapContainer>();
    public StorytellerComp_HumanThreatCycle ThreatCycle;
    public StoryQueue StoryQueue = new StoryQueue();
    public StoryStatus StoryStatus = new StoryStatus();
    public StoryOverlay StoryOverlay = new StoryOverlay();

    private Map _firstMapOfPlayer;

    public Map FirstMapOfPlayer {
        get => _firstMapOfPlayer ?? Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
        set => _firstMapOfPlayer = value;
    }

    private Map _lastColonizedMap;

    public Map LastColonizedMap {
        get => _lastColonizedMap ?? FirstMapOfPlayer;
        set => _lastColonizedMap = value;
    }

    private Map _sameAsLastEvent;

    public Map SameAsLastEvent {
        get => _sameAsLastEvent ?? FirstMapOfPlayer;
        set => _sameAsLastEvent = value;
    }

    private List<string> reservedPawnKeysWorkingList;
    private List<string> reservedPawnGroupKeysWorkingList;
    private List<Pawn> reservedPawnValuesWorkingList;
    private List<PawnGroup> reservedPawnGroupValuesWorkingList;
    private List<string> reservedMapKeysWorkingList;
    private List<MapContainer> reservedMapValuesWorkingList;

    public StoryComponent(Game game) {
        Tell.Log("Initialized StoryComponent");
    }

    public void Reset() {
        Initialised = false;
        StoryArc = null;
        ForcedUpdate = false;
        PersistentMaps = new List<Map>();
        ShotReportBank = new List<ReportBank>();
        VariableBank = new Dictionary<string, float>();
        PawnBank = new Dictionary<string, Pawn>();
        PawnGroupBank = new Dictionary<string, PawnGroup>();
        MapBank = new Dictionary<string, MapContainer>();
        ThreatCycle = null;
        _firstMapOfPlayer = null;
        _sameAsLastEvent = null;
        _lastColonizedMap = null;
        StoryQueue = new StoryQueue();
        StoryOverlay = new StoryOverlay();
        StoryStatus = new StoryStatus();
    }

    public override void GameComponentUpdate() {
        if (StoryArc == null || !Initialised) {
            return;
        }

        FollowThing();
        ChangeWorldView();
    }

    private void ChangeWorldView() {
        if (!StoryStatus.DisableCameraControls) {
            return;
        }

        Find.World.renderer.wantedMode = StoryStatus.ShowWorld ? WorldRenderMode.Planet : WorldRenderMode.None;
    }

    private void FollowThing() {
        if (StoryStatus.FollowThing == GlobalTargetInfo.Invalid) return;
        var target = CameraJumper.GetAdjustedTarget(StoryStatus.FollowThing);
        if (!target.IsValid) {
            Tell.Log("Invalid thing to follow, resetting...");
            StoryStatus.FollowThing = GlobalTargetInfo.Invalid;
            return;
        }

        if (target.HasThing) {
            if (Current.ProgramState != ProgramState.Playing)
                return;
            Map mapHeld = target.Thing.MapHeld;
            if (mapHeld == null || !Find.Maps.Contains(mapHeld) || !target.Thing.DrawPos.InBounds(mapHeld))
                return;
            if (Find.CurrentMap != mapHeld) {
                Current.Game.CurrentMap = mapHeld;
            }

            StoryStatus.JumpException = true;
            Find.CameraDriver.JumpToCurrentMapLoc(target.Thing.DrawPos);
            StoryStatus.JumpException = false;
        } else {
            StoryStatus.JumpException = true;
            CameraJumper.TryJump(target);
            StoryStatus.JumpException = false;

        }
    }

    public override void GameComponentOnGUI() {
        try {
            if (HumanStoryteller.HumanStorytellerGame && HumanStoryteller.StoryComponent.Initialised) {
                HumanStoryteller.StoryComponent?.StoryOverlay?.DrawOverlay();
            }

            if (!HumanStoryteller.CreatorTools || !HumanStoryteller.StoryComponent.Initialised) return;
            if (UI.MouseCell().InBounds(Find.CurrentMap)) {
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(5, 5, 400, 30),
                    "tile:" + UI.MouseCell().x + ":" + UI.MouseCell().y + ":" + UI.MouseCell().z + " (storymaker info)");
            }
        } catch (Exception e) {
            Tell.Err("Error while drawing overlay, " + e.Message, e);
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Initialised, "initialised");
        Scribe_Deep.Look(ref StoryArc, "story");
        Scribe_Deep.Look(ref StoryOverlay, "storyOverlay");
        Scribe_Deep.Look(ref StoryStatus, "storyStatus");
        Scribe_Deep.Look(ref StoryQueue, "storyQueue");
        Scribe_Values.Look(ref StoryId, "storyId");
        Scribe_Collections.Look(ref ShotReportBank, "shotReportBank", LookMode.Deep);
        Scribe_Collections.Look(ref PersistentMaps, "persistentMaps", LookMode.Reference);
        Scribe_Collections.Look(ref VariableBank, "variableBank", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref PawnBank, "pawnBank", LookMode.Value, LookMode.Reference, ref reservedPawnKeysWorkingList,
            ref reservedPawnValuesWorkingList);
        Scribe_Collections.Look(ref PawnGroupBank, "pawnGroupBank", LookMode.Value, LookMode.Deep, ref reservedPawnGroupKeysWorkingList,
            ref reservedPawnGroupValuesWorkingList);
        Scribe_Collections.Look(ref MapBank, "mapBank", LookMode.Value, LookMode.Deep, ref reservedMapKeysWorkingList,
            ref reservedMapValuesWorkingList);
        if (!Find.Maps.Contains(_firstMapOfPlayer)) {
            _firstMapOfPlayer = null;
        }
        if (!Find.Maps.Contains(_sameAsLastEvent)) {
            _sameAsLastEvent = null;
        }
        if (!Find.Maps.Contains(_lastColonizedMap)) {
            _lastColonizedMap = null;
        }
        Scribe_References.Look(ref _firstMapOfPlayer, "firstMapOfPlayer");
        Scribe_References.Look(ref _sameAsLastEvent, "sameAsLastEvent");
        Scribe_References.Look(ref _lastColonizedMap, "lastColonizedMap");
        if (Scribe.mode == LoadSaveMode.LoadingVars && HumanStoryteller.CreatorTools) {
            Tell.Log("StoryComponent Loaded:" + ToString());
        }
    }

    public override string ToString() {
        return
            $"Initialised: [{Initialised}], Story: [{StoryArc}], StoryId: [{StoryId}], VariableBank: [{VariableBank.Join()}], PawnBank: [{PawnBank.Join()}], MapBank: [{MapBank.Join()}], ThreatCycle: [{ThreatCycle}], FirstMapOfPlayer: [{_firstMapOfPlayer}], SameAsLastEvent: [{_sameAsLastEvent}], ReservedPawnKeysWorkingList: [{reservedPawnKeysWorkingList}], ReservedPawnValuesWorkingList: [{reservedPawnValuesWorkingList}], ReservedMapKeysWorkingList: [{reservedMapKeysWorkingList}], ReservedMapValuesWorkingList: [{reservedMapValuesWorkingList}], QueueSize: [{StoryQueue.Size()}]";
    }
}