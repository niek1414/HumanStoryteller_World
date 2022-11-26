using System;
using HumanStoryteller.Model.Incident;
using Verse;
using Verse.AI;

namespace HumanStoryteller.Model.PawnGroup.Filter; 
public class CanReachFilter : PawnGroupFilter {
    public const String Name = "CanReach";

    public Location Target;
    
    public CanReachFilter() {
    }

    protected override bool Filter(Pawn p, Map map) {
        var cell = Target.GetSingleCell(map);
        return p.CanReach(cell, cell.Impassable(map) ? PathEndMode.Touch : PathEndMode.OnCell, Danger.Deadly);
    }
    
    protected override void ExposeFilter() {
        Scribe_Deep.Look(ref Target, "target");
    }

    public override string ToString() {
        return $"{base.ToString()}, Target: [{Target}]";
    }
}