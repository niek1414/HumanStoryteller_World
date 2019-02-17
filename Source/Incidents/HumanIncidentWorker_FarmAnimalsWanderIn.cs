using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_FarmAnimalsWanderIn : HumanIncidentWorker {
        public const String Name = "FarmAnimalsWanderIn";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_FarmAnimalsWanderIn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_FarmAnimalsWanderIn allParams = Tell.AssertNotNull((HumanIncidentParams_FarmAnimalsWanderIn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) {
                result = CellFinder.RandomEdgeCell(map);
            }

            PawnKindDef kind = null;
            if (allParams.Kind != "") {
                kind = (from x in DefDatabase<PawnKindDef>.AllDefs
                    where x.RaceProps.Animal && x.defName == allParams.Kind
                    select x).First();
            }

            if (kind == null && !TryFindRandomPawnKind(map, out kind)) {
                Tell.Err("Failed to find pawn kind for animal to join.");
                return ir;
            }

            int num;

            if (allParams.Amount != -1) {
                num = (int) allParams.Amount;
            } else {
                num = Mathf.Clamp(GenMath.RoundRandom(2.5f / kind.RaceProps.baseBodySize), 2, 10);
            }

            for (int i = 0; i < num; i++) {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 12);
                Pawn pawn = PawnGenerator.GeneratePawn(kind);
                if (i < allParams.Names.Count) {
                    pawn.Name = new NameSingle(allParams.Names[i]);
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

    public class HumanIncidentParams_FarmAnimalsWanderIn : HumanIncidentParms {
        public long Amount;
        public List<string> Names;
        public String Kind;

        public HumanIncidentParams_FarmAnimalsWanderIn() {
        }

        public HumanIncidentParams_FarmAnimalsWanderIn(String target, HumanLetter letter, long amount = -1, List<string> names = null,
            String kind = "") :
            base(target, letter) {
            Amount = amount;
            Names = names ?? new List<string>();
            Kind = kind;
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, Names: {Names}, Kind: {Kind}";
        }
        
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Names, "names");
            Scribe_Values.Look(ref Kind, "kind");
        }
    }
}