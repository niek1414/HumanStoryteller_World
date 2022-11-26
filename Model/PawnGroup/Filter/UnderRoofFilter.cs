using System;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter; 
public class UnderRoofFilter : PawnGroupFilter {
    public const String Name = "UnderRoof";

    public UnderRoofFilter() {
    }

    protected override bool Filter(Pawn p, Map map) {
        return p.Position.Roofed(p.Map);
    }
    
    protected override void ExposeFilter() {
    }

    public override string ToString() {
        return $"{base.ToString()}";
    }
}