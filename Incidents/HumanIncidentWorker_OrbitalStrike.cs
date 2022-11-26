using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_OrbitalStrike : HumanIncidentWorker {
    public const String Name = "OrbitalStrike";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_OrbitalStrike)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_OrbitalStrike allParams =
            Tell.AssertNotNull((HumanIncidentParams_OrbitalStrike) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        IntVec3 cell = allParams.Location.GetSingleCell(map);
        
        switch (allParams.OrbitalType) {
            case "Bombardment":
                Bombardment bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, cell, map);
                bombardment.duration = 540;
                bombardment.instigator = null;
                bombardment.weaponDef = null;
                bombardment.StartStrike();
                break;
            case "PowerBeam":
                PowerBeam powerBeam = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, cell, map);
                powerBeam.duration = 600;
                powerBeam.instigator = null;
                powerBeam.weaponDef = null;
                powerBeam.StartStrike();
                break;
            default:
                Thing thing = ThingMaker.MakeThing(ThingDef.Named(allParams.OrbitalType));
                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Direct);
                break;
        }
        
        SendLetter(@params);

        return ir;
    }
}

public class HumanIncidentParams_OrbitalStrike : HumanIncidentParams {
    public string OrbitalType = "";
    public Location Location = new Location();

    public HumanIncidentParams_OrbitalStrike() {
    }

    public HumanIncidentParams_OrbitalStrike(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Location: [{Location}], OrbitalType: [{OrbitalType}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Location, "location");
        Scribe_Values.Look(ref OrbitalType, "orbitalType");
    }
}