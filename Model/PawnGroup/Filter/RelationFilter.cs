using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HumanStoryteller.Model.PawnGroup.Filter; 
public class RelationFilter : PawnGroupFilter {
    public const String Name = "Relation";

    public List<FactionRelationKind> Type = new List<FactionRelationKind>();

    public RelationFilter() {
    }

    protected override bool Filter(Pawn p, Map map) {
        return Type.Contains(Faction.OfPlayer == p.Faction ? FactionRelationKind.Ally : Faction.OfPlayer.RelationWith(p.Faction, true)?.kind ?? FactionRelationKind.Neutral);
    }
    
    protected override void ExposeFilter() {
        Scribe_Collections.Look(ref Type, "type", LookMode.Value);
    }

    public override string ToString() {
        return $"{base.ToString()}, Type: [{Type}]";
    }
}