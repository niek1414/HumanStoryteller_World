using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_ShipPartCrash : HumanIncidentWorker {
    public const String Name = "ShipPartCrash";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_ShipPartCrash)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_ShipPartCrash
            allParams = Tell.AssertNotNull((HumanIncidentParams_ShipPartCrash) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        ThingDef shipPartDef;
        if (allParams.ShipCrashedPart != "") {
            shipPartDef = ThingDef.Named(allParams.ShipCrashedPart);
        } else {
            shipPartDef = ThingDefOf.MechCapsule;
        }

        List<TargetInfo> list = new List<TargetInfo>();
        IntVec3 intVec = MechClusterUtility.FindDropPodLocation(map, delegate(IntVec3 spot)
        {
            if (!spot.Fogged(map) && GenConstruct.CanBuildOnTerrain(shipPartDef, spot, map, Rot4.North))
            {
                return GenConstruct.CanBuildOnTerrain(shipPartDef, new IntVec3(spot.x - Mathf.CeilToInt((float)shipPartDef.size.x / 2f), spot.y, spot.z), map, Rot4.North);
            }
            return false;
        });
        if (intVec == IntVec3.Invalid)
        {
            Tell.Warn("ShipPartCrash is trying to use a invalid location");
        }
        float points = Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(map) * 0.9f, 300f);
        List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
        {
            groupKind = PawnGroupKindDefOf.Combat,
            tile = map.Tile,
            faction = Faction.OfMechanoids,
            points = points
        }).ToList();
        Thing thing = ThingMaker.MakeThing(shipPartDef);
        thing.SetFaction(Faction.OfMechanoids);
        LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenMechanoidsDefend(new List<Thing>
        {
            thing
        }, Faction.OfMechanoids, 28f, intVec, false, false), map, list2);
        DropPodUtility.DropThingsNear(intVec, map, list2);
        foreach (Pawn item in list2)
        {
            item.TryGetComp<CompCanBeDormant>()?.ToSleep();
        }
        list.AddRange(from p in list2
            select new TargetInfo(p));
        GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, thing), intVec, map);
        list.Add(new TargetInfo(intVec, map));

        if (shipPartDef == ThingDefOf.ShipChunk) {
            SendLetter(allParams, ThingDefOf.ShipChunk.label, "MessageShipChunkDrop".Translate(), LetterDefOf.PositiveEvent, list);
        } else {
            SendLetter(allParams, shipPartDef.label, shipPartDef.description, LetterDefOf.ThreatSmall, list);
        }

        return ir;
    }
}

public class HumanIncidentParams_ShipPartCrash : HumanIncidentParams {
    public Number Amount = new Number(1);
    public string ShipCrashedPart = "";

    public HumanIncidentParams_ShipPartCrash() {
    }

    public HumanIncidentParams_ShipPartCrash(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Amount: [{Amount}], ShipCrashedPart: [{ShipCrashedPart}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Amount, "amount");
        Scribe_Values.Look(ref ShipCrashedPart, "shipCrashedPart");
    }
}