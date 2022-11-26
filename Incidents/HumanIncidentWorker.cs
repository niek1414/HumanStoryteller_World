using System;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents; 
public abstract class HumanIncidentWorker : IExposable {
    private void PreExecute(HumanIncidentParams @params) {
        HumanStoryteller.StoryComponent.SameAsLastEvent = (Map) @params.GetTarget();
        MapUtil.GetMapContainerByTile(@params.Target.GetTileFromTarget(false), false)?.FakeConnect();
    }

    private void PostExecute(HumanIncidentParams @params, IncidentResult incidentResult) {
        MapUtil.GetMapContainerByTile(@params.Target.GetTileFromTarget(false), false)?.FakeDisconnect();
        incidentResult.Target = @params.Target;
    }

    public IncidentResult ExecuteIncident(HumanIncidentParams @params) {
        PreExecute(@params);
        IncidentResult ir = Execute(@params);
        PostExecute(@params, ir);
        return ir;
    }

    public virtual void PreLoad(HumanIncidentParams @params) {
        //Can be overridden
    }

    protected abstract IncidentResult Execute(HumanIncidentParams @params);

    protected void SendLetter(HumanIncidentParams @params, String title, String message, LetterDef type, LookTargets target,
        Faction relatedFaction = null, string debugInfo = null) {
        if (@params.Letter != null) {
            Letter l;
            if (@params.Letter.Type == null) {
                l = LetterMaker.MakeLetter(title, message, type, target, relatedFaction);
            } else {
                if (@params.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }

                l = LetterMaker.MakeLetter(@params.Letter.Title.Get(), @params.Letter.Message.Get(), @params.Letter.Type, target, relatedFaction);
                if (@params.Letter.Force) {
                    l.OpenLetter();
                }
            }

            Find.LetterStack.ReceiveLetter(l, debugInfo);
        }
    }

    protected void SendLetter(HumanIncidentParams @params, LookTargets target = null, Faction relatedFaction = null, string debugInfo = null) {
        if (@params.Letter?.Type != null) {
            if (@params.Letter.Shake) {
                Find.CameraDriver.shaker.DoShake(4f);
            }

            var letter = LetterMaker.MakeLetter(@params.Letter.Title.Get(), @params.Letter.Message.Get(), @params.Letter.Type, target, relatedFaction);
            if (@params.Letter.Force) {
                letter.OpenLetter();
            }

            Find.LetterStack.ReceiveLetter(letter, debugInfo);
        }
    }

    public virtual void ExposeData() {
    }
}

public class Target : IExposable {
    public string CustomTarget;
    public string TargetPreset;
    public long TargetTile;
    public string TargetName;

    public int GetTileFromTarget(bool warn = true) {
        switch (CustomTarget) {
            case "Preset":
                return GetPresetTarget().Tile;
            case "Tile":
                return (int) TargetTile;
            case "Name":
                return GetNamedTarget(warn).Tile;
            default:
                return GetPresetTarget().Tile;
        }
    }

    public Map GetMapFromTarget(bool warn = true) {
        switch (CustomTarget) {
            case "Preset":
                return GetPresetTarget();
            case "Tile":
                return GetTileTarget(warn);
            case "Name":
                return GetNamedTarget(warn);
            default:
                Tell.Warn("Unknown map target type", CustomTarget);
                return GetPresetTarget();
        }
    }

    private Map GetNamedTarget(bool warn = true) {
        return MapUtil.GetMapByName(TargetName, warn) ?? MapUtil.FirstOfPlayer();
    }

    private Map GetTileTarget(bool warn = true) {
        return Current.Game.FindMap((int) TargetTile) ?? MapUtil.GetMapByTile(TargetTile, warn) ?? MapUtil.FirstOfPlayer();
    }
    
    private Map GetPresetTarget() {
        switch (TargetPreset) {
            case "FirstOfPlayer":
                return MapUtil.FirstOfPlayer();
            case "RandomOfPlayer":
                return Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();
            case "SameAsLastEvent":
                return MapUtil.SameAsLastEvent();
            case "LastColonized":
                return MapUtil.LastColonized();
            default:
                return MapUtil.FirstOfPlayer();
        }
    }

    public override string ToString() {
        var result = CustomTarget + ": ";
        switch (CustomTarget) {
            case "Preset":
                result += TargetPreset;
                break;
            case "Tile":
                result += TargetTile.ToString();
                break;
            case "Name":
                result += TargetName;
                break;
            default:
                result += "Unknown custom type: " + CustomTarget;
                break;
        }

        return result;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref CustomTarget, "customTarget");
        Scribe_Values.Look(ref TargetPreset, "targetPreset");
        Scribe_Values.Look(ref TargetTile, "targetTile");
        Scribe_Values.Look(ref TargetName, "targetName");
    }
}

public class IncidentResult : IExposable, ILoadReferenceable {
    private int _id;
    public Target Target;

    public IncidentResult() {
        _id = Rand.Int;
    }

    public int Id => _id;

    public virtual void ExposeData() {
        Scribe_Values.Look(ref _id, "id");
        Scribe_Deep.Look(ref Target, "target");
    }

    public override string ToString() {
        return $"Target: {Target}, Id: {Id}";
    }

    public string GetUniqueLoadID() {
        return $"IncidentResult_{_id}";
    }
}