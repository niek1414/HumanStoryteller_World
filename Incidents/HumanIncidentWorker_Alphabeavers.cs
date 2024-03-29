using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Alphabeavers : HumanIncidentWorker {
    public const String Name = "Alphabeavers";

    private static readonly FloatRange CountPerColonistRange = new FloatRange(1f, 1.5f);

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_Alphabeavers)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_Alphabeavers allParams = Tell.AssertNotNull((HumanIncidentParams_Alphabeavers) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        PawnKindDef alphabeaver = PawnKindDefOf.Alphabeaver;
        if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) {
            result = CellFinder.RandomEdgeCell(map);
        }

        int num;
        var amount = allParams.Amount.GetValue();
        if (amount != -1) {
            num = Mathf.RoundToInt(amount);
        } else {
            int freeColonistsCount = map.mapPawns.FreeColonistsCount;
            float randomInRange = CountPerColonistRange.RandomInRange;
            float f = freeColonistsCount * randomInRange;
            num = Mathf.Clamp(GenMath.RoundRandom(f), 1, 10);
        }

        for (int i = 0; i < num; i++) {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
            Pawn newThing = PawnGenerator.GeneratePawn(alphabeaver);
            Pawn pawn = (Pawn) GenSpawn.Spawn(newThing, loc, map);
            pawn.needs.food.CurLevelPercentage = 1f;
        }


        SendLetter(allParams, "LetterLabelBeaversArrived".Translate(), "BeaversArrived".Translate(), LetterDefOf.ThreatSmall,
            new TargetInfo(result, map));

        return ir;
    }
}

public class HumanIncidentParams_Alphabeavers : HumanIncidentParams {
    public Number Amount = new Number();

    public HumanIncidentParams_Alphabeavers() {
    }

    public HumanIncidentParams_Alphabeavers(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Amount: [{Amount}]";
    }
    
    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Amount, "amount");
    }
}