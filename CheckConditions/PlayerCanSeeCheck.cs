using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions; 
public class PlayerCanSeeCheck : CheckCondition {
    public const String Name = "PlayerCanSee";
    private SeeConditions _seeConditions;
    private Location _location;

    public PlayerCanSeeCheck() {
    }

    public PlayerCanSeeCheck(SeeConditions seeConditions, Location location) {
        _seeConditions = Tell.AssertNotNull(seeConditions, nameof(seeConditions), GetType().Name);
        _location = Tell.AssertNotNull(location, nameof(location), GetType().Name);
    }

    public override bool Check(IncidentResult result, int checkPosition) {
        var target = result.Target.GetMapFromTarget();
        var cell = _location.GetSingleCell(target, false);
        if (!cell.IsValid) {
            return false;
        }

        switch (_seeConditions) {
            case SeeConditions.OnlyFog:
                return !target.fogGrid.IsFogged(cell);
            case SeeConditions.OnlyViewport:
                return Find.CameraDriver.CurrentViewRect.Contains(cell);
            case SeeConditions.FogAndViewport:
                return !target.fogGrid.IsFogged(cell) && Find.CameraDriver.CurrentViewRect.Contains(cell);
            default:
                throw new ArgumentOutOfRangeException("Unknown SeeCondition: " + _seeConditions);
        }
    }

    public override string ToString() {
        return $"SeeConditions: [{_seeConditions}], Location: [{_location}]";
    }
    
    public enum SeeConditions {
        OnlyFog,
        OnlyViewport,
        FogAndViewport
    }
    
    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref _seeConditions, "seeConditions");
        Scribe_Deep.Look(ref _location, "location");
    }
}