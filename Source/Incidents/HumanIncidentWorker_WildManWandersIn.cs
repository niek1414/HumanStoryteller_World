using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_WildManWandersIn : HumanIncidentWorker {
        public const String Name = "WildManWandersIn";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_WildManWandersIn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_WildManWandersIn
                allParams = Tell.AssertNotNull((HumanIncidentParams_WildManWandersIn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            if (!CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 cell))
            {
                return ir;
            }
            if (!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction formerFaction, false, true))
            {
                return ir;
            }
            Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.WildMan, formerFaction);
            pawn.SetFaction(null);
            if (allParams.Name != "") {
                switch (pawn.Name) {
                    case NameTriple prevNameTriple:
                        pawn.Name = new NameTriple(allParams.Name, allParams.Name, prevNameTriple.Last);
                        break;
                    case NameSingle prevNameSingle:
                        pawn.Name = new NameTriple(allParams.Name, allParams.Name, prevNameSingle.Name);
                        break;
                    default:
                        pawn.Name = new NameTriple(allParams.Name, allParams.Name, "");
                        break;
                }
            }
            GenSpawn.Spawn(pawn, cell, map);
            IncidentDef def = IncidentDef.Named("WildManWandersIn");
            string title = def.letterLabel.Formatted(pawn.LabelShort, pawn.Named("PAWN"));
            string text = def.letterText.Formatted(pawn.LabelShort, pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
            SendLetter(allParams, title, text, def.letterDef, pawn);
            
            return ir;
        }
    }

    public class HumanIncidentParams_WildManWandersIn : HumanIncidentParms {
        public string Name;

        public HumanIncidentParams_WildManWandersIn() {
        }

        public HumanIncidentParams_WildManWandersIn(String target, HumanLetter letter, string name = "") : base(target, letter) {
            Name = name;
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: {Name}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Name, "name");
        }
    }
}