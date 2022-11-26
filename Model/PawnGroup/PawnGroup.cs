using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace HumanStoryteller.Model.PawnGroup; 
public class PawnGroup : IExposable {
    public List<Pawn> Pawns = new List<Pawn>();

    public PawnGroup() {
    }

    public PawnGroup(List<Pawn> pawns) {
        Pawns = pawns;
    }

    public bool Prune() {
        Pawns = Pawns.Where(p => p.Discarded || p.Destroyed).ToList();
        return Pawns.NullOrEmpty();
    }

    public void ExposeData() {
        Scribe_Collections.Look(ref Pawns, "pawns", LookMode.Reference);
    }

    public void Add(Pawn pawn) {
        Pawns.Add(pawn);
    }

    public void Add(PawnGroup pawnGroup) {
        pawnGroup.Pawns.ForEach(p => Pawns.Add(p));
    }

    public override string ToString() {
        return $"Kinds: [{Pawns.Join()}]";
    }
}