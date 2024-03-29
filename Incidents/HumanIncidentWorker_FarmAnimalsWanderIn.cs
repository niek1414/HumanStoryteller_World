using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_FarmAnimalsWanderIn : HumanIncidentWorker {
    public const String Name = "FarmAnimalsWanderIn";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_FarmAnimalsWanderIn)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_FarmAnimalsWanderIn allParams = Tell.AssertNotNull((HumanIncidentParams_FarmAnimalsWanderIn) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) {
            result = CellFinder.RandomEdgeCell(map);
        }

        PawnKindDef kind = null;
        if (allParams.AnimalKind != "") {
            kind = (from x in DefDatabase<PawnKindDef>.AllDefs
                where x.RaceProps.Animal && x.defName == allParams.AnimalKind
                select x).First();
        }

        if (kind == null && !TryFindRandomPawnKind(map, out kind)) {
            Tell.Err("Failed to find pawn kind for animal to join.");
            return ir;
        }

        int num;

        var amount = allParams.Amount.GetValue();
        if (amount != -1) {
            num = Mathf.RoundToInt(amount);
        } else {
            num = Mathf.Clamp(GenMath.RoundRandom(2.5f / kind.RaceProps.baseBodySize), 2, 10);
        }

        for (int i = 0; i < num; i++) {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 12);
            Pawn pawn = PawnGenerator.GeneratePawn(kind);
            if (i < allParams.OutNames.Count) {
                PawnUtil.SetDisplayName(pawn, allParams.OutNames[i]);
                PawnUtil.SavePawnByName(allParams.OutNames[i], pawn);
            }

            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
            pawn.SetFaction(Faction.OfPlayer);
        }

        SendLetter(allParams, "LetterLabelFarmAnimalsWanderIn".Translate(kind.GetLabelPlural()).CapitalizeFirst(),
            "LetterFarmAnimalsWanderIn".Translate(kind.GetLabelPlural()), LetterDefOf.PositiveEvent, new TargetInfo(result, map));
        return ir;
    }

    private bool TryFindRandomPawnKind(Map map, out PawnKindDef kind) {
        return (from x in DefDatabase<PawnKindDef>.AllDefs
            where x.RaceProps.Animal && x.RaceProps.wildness < 0.35f && map.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race)
            select x).TryRandomElementByWeight(k => 0.420000017f - k.RaceProps.wildness, out kind);
    }
}

public class HumanIncidentParams_FarmAnimalsWanderIn : HumanIncidentParams {
    public Number Amount = new Number();
    public List<string> OutNames = new List<string>();
    public String AnimalKind = "";

    public HumanIncidentParams_FarmAnimalsWanderIn() {
    }

    public HumanIncidentParams_FarmAnimalsWanderIn(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Amount: [{Amount}], Names: [{OutNames.ToCommaList()}], Kind: [{AnimalKind}]";
    }
    
    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Amount, "amount");
        Scribe_Collections.Look(ref OutNames, "names", LookMode.Value);
        Scribe_Values.Look(ref AnimalKind, "animalKind");
    }
}