using System;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_CropBlight : HumanIncidentWorker {
    public const String Name = "CropBlight";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_CropBlight)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_CropBlight allParams =
            Tell.AssertNotNull((HumanIncidentParams_CropBlight) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        if (!TryFindRandomBlightablePlant(map, out Plant plant)) {
            return ir;
        }

        Room room = plant.GetRoom();
        plant.CropBlighted();
        int i = 0;
        for (int num = GenRadial.NumCellsInRadius(Mathf.Min(allParams.Radius.GetValue(), 56)); i < num; i++) {
            IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
            if (intVec.InBounds(map) && intVec.GetRoom(map) == room) {
                Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
                if (firstBlightableNowPlant != null && firstBlightableNowPlant != plant &&
                    Rand.Chance(allParams.Chance.GetValue() * BlightChanceFactor(firstBlightableNowPlant.Position, plant.Position))) {
                    firstBlightableNowPlant.CropBlighted();
                }
            }
        }

        Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), "LetterCropBlight".Translate(), LetterDefOf.NegativeEvent,
            new TargetInfo(plant.Position, map));
        return ir;
    }

    private bool TryFindRandomBlightablePlant(Map map, out Plant plant) {
        Thing result;
        bool result2 = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
            where ((Plant) x).BlightableNow
            select x).TryRandomElement(out result);
        plant = (Plant) result;
        return result2;
    }

    private float BlightChanceFactor(IntVec3 c, IntVec3 root) {
        return Mathf.InverseLerp(15f, 7.5f, c.DistanceTo(root));
    }
}

public class HumanIncidentParams_CropBlight : HumanIncidentParams {
    public Number Radius = new Number(15);
    public Number Chance = new Number(0.4f);

    public HumanIncidentParams_CropBlight() {
    }

    public HumanIncidentParams_CropBlight(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Radius: [{Radius}], Chance: [{Chance}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Radius, "radius");
        Scribe_Deep.Look(ref Chance, "chance");
    }
}