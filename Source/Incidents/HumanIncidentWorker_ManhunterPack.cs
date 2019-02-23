using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ManhunterPack : HumanIncidentWorker {
        public const String Name = "ManhunterPack";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_ManhunterPack)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ManhunterPack allParams =
                Tell.AssertNotNull((HumanIncidentParams_ManhunterPack) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            PawnKindDef kindDef;

            float points = allParams.Points >= 0
                ? StorytellerUtility.DefaultThreatPointsNow(map) * allParams.Points
                : StorytellerUtility.DefaultThreatPointsNow(map);

            if (allParams.Kind != "") {
                kindDef = PawnKindDef.Named(allParams.Kind);
            } else {
                if (!ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(points, map.Tile, out PawnKindDef animalKind)) {
                    return ir;
                }

                kindDef = animalKind;
            }

            if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 result, map, CellFinder.EdgeRoadChance_Animal)) {
                return ir;
            }

            List<Pawn> list = ManhunterPackIncidentUtility.GenerateAnimals(kindDef, map.Tile, points * 1f);
            Rot4 rot = Rot4.FromAngleFlat((map.Center - result).AngleFlat);
            for (int i = 0; i < list.Count; i++) {
                Pawn pawn = list[i];
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(result, map, 10);
                GenSpawn.Spawn(pawn, loc, map, rot);
                pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                pawn.mindState.exitMapAfterTick = Find.TickManager.TicksGame + Rand.Range(60000, 120000);
            }

            SendLetter(allParams, "LetterLabelManhunterPackArrived".Translate(), "ManhunterPackArrived".Translate(kindDef.GetLabelPlural()),
                LetterDefOf.ThreatBig, list.Count < 1 ? null : list[0]);

            Find.TickManager.slower.SignalForceNormalSpeedShort();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.ForbiddingDoors, OpportunityType.Critical);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.AllowedAreas, OpportunityType.Important);
            return ir;
        }
    }

    public class HumanIncidentParams_ManhunterPack : HumanIncidentParms {
        public string Kind;
        public float Points;

        public HumanIncidentParams_ManhunterPack() {
        }

        public HumanIncidentParams_ManhunterPack(String target, HumanLetter letter, string kind = "", float points = -1) :
            base(target, letter) {
            Points = points;
            Kind = kind;
        }

        public override string ToString() {
            return $"{base.ToString()}, Points: {Points}, Kind: {Kind}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Points, "points");
            Scribe_Values.Look(ref Kind, "kind");
        }
    }
}