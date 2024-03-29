using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Infestation : HumanIncidentWorker {
    public const String Name = "Infestation";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_Infestation)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_Infestation allParams = Tell.AssertNotNull((HumanIncidentParams_Infestation) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        var paramsPoints = allParams.Points.GetValue();
        float points = paramsPoints >= 0
            ? StorytellerUtility.DefaultThreatPointsNow(map) * paramsPoints
            : StorytellerUtility.DefaultThreatPointsNow(map);
        
        Thing t = SpawnTunnels(Mathf.Max(GenMath.RoundRandom(points / 220f), 1), map);
        IncidentDef def = IncidentDef.Named(Name);
        SendLetter(allParams, def.letterLabel, string.Format(def.letterText).CapitalizeFirst(), def.letterDef, t);
        Find.TickManager.slower.SignalForceNormalSpeedShort();

        return ir;
    }

    private Thing SpawnTunnels(int hiveCount, Map map) {
        if (!InfestationCellFinder.TryFindCell(out IntVec3 cell, map)) {
            return null;
        }

        Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
        for (int i = 0; i < hiveCount - 1; i++) {
            cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive,
                ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), false, true);
            if (cell.IsValid) {
                thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
            }
        }

        return thing;
    }
}

public class HumanIncidentParams_Infestation : HumanIncidentParams {
    public Number Points = new Number();

    public HumanIncidentParams_Infestation() {
    }

    public HumanIncidentParams_Infestation(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Points: [{Points}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Points, "points");
    }
}