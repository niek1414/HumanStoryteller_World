using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_TransferPawn : HumanIncidentWorker {
    public const String Name = "TransferPawn";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_TransferPawn)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_TransferPawn allParams = Tell.AssertNotNull((HumanIncidentParams_TransferPawn) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        IntVec3 enterCell = CellFinder.RandomEdgeCell(map);

        foreach (var pawn in allParams.Pawns.FilterEnumerable(map)) {
            if (pawn.Dead) continue;
            IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(enterCell, map, 4);
            if (pawn.Spawned) {
                pawn.DeSpawn();
            }
            GenSpawn.Spawn(pawn, loc, map);
        }

        SendLetter(@params);

        return ir;
    }
}

public class HumanIncidentParams_TransferPawn : HumanIncidentParams {
    public PawnGroupSelector Pawns;

    public HumanIncidentParams_TransferPawn() {
    }

    public HumanIncidentParams_TransferPawn(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Pawns: [{Pawns}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Pawns, "names");
    }
}