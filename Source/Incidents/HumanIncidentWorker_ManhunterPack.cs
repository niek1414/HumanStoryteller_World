using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ManhunterPack : HumanIncidentWorker {
        public const String Name = "ManhunterPack";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
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

            var paramsPoints = allParams.Points.GetValue();
            float points = paramsPoints >= 0
                ? StorytellerUtility.DefaultThreatPointsNow(map) * paramsPoints
                : StorytellerUtility.DefaultThreatPointsNow(map);

            if (allParams.AnimalKind != "") {
                kindDef = PawnKindDef.Named(allParams.AnimalKind);
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
        public string AnimalKind = "";
        public Number Points = new Number();

        public HumanIncidentParams_ManhunterPack() {
        }

        public HumanIncidentParams_ManhunterPack(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Points: [{Points}], Kind: [{AnimalKind}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Values.Look(ref AnimalKind, "animalKind");
        }
    }
}