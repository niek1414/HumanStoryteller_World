using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_ResourcePodCrash : HumanIncidentWorker {
    public const String Name = "ResourcePodCrash";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_ResourcePodCrash)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_ResourcePodCrash allParams =
            Tell.AssertNotNull((HumanIncidentParams_ResourcePodCrash) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        List<Thing> things = allParams.Item.GetThings(true) ?? ThingSetMakerDefOf.ResourcePod.root.Generate();

        IntVec3 intVec = allParams.Location.GetSingleCell(map);

        DropPodUtility.DropThingsNear(intVec, map, things, 110, allParams.InstaPlace, true);
        SendLetter(allParams, "LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent,
            new TargetInfo(intVec, map));

        return ir;
    }
}

public class HumanIncidentParams_ResourcePodCrash : HumanIncidentParams {
    public Item Item = new Item("", "", "", new Number(20));
    public bool InstaPlace;
    public Location Location = new Location();

    public HumanIncidentParams_ResourcePodCrash() {
    }

    public HumanIncidentParams_ResourcePodCrash(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Item: [{Item}], InstaPlace: [{InstaPlace}], Location: [{Location}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Item, "item");
        Scribe_Values.Look(ref InstaPlace, "instaPlace");
        Scribe_Deep.Look(ref Location, "location");
    }
}