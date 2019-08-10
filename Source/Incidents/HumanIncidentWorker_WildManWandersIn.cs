using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_WildManWandersIn : HumanIncidentWorker {
        public const String Name = "WildManWandersIn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
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
            if (allParams.Gender != ""){
                pawn.gender = PawnUtil.GetGender(allParams.Gender);
            }
            if (allParams.OutName != "") {
                PawnUtil.SavePawnByName(allParams.OutName, pawn);
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
        public string OutName = "";
        public string Gender = "";

        public HumanIncidentParams_WildManWandersIn() {
        }

        public HumanIncidentParams_WildManWandersIn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: {OutName}, Gender: {Gender}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref OutName, "name");
            Scribe_Values.Look(ref Gender, "gender");
        }
    }
}