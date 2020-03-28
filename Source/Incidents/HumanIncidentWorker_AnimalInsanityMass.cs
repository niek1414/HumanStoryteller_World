using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_AnimalInsanityMass : HumanIncidentWorker {
        public const String Name = "AnimalInsanityMass";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_AnimalInsanityMass)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_AnimalInsanityMass allParams =
                Tell.AssertNotNull((HumanIncidentParams_AnimalInsanityMass) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            var parmPoints = allParams.Points.GetValue();
            float points = parmPoints >= 0
                ? StorytellerUtility.DefaultThreatPointsNow(map) * parmPoints
                : StorytellerUtility.DefaultThreatPointsNow(map);

            float adjustedPoints = points;
            if (adjustedPoints > 250f) {
                adjustedPoints -= 250f;
                adjustedPoints *= 0.5f;
                adjustedPoints += 250f;
            }

            IEnumerable<PawnKindDef> source = null;
            if (allParams.AnimalKind != "") {
                source = from x in DefDatabase<PawnKindDef>.AllDefs
                    where x.RaceProps.Animal && x.defName == allParams.AnimalKind
                    select x;
            }

            if (source == null || !source.Any()) {
                source = from def in DefDatabase<PawnKindDef>.AllDefs
                    where def.RaceProps.Animal && def.combatPower <= adjustedPoints && (from p in map.mapPawns.AllPawnsSpawned
                              where p.kindDef == def && AnimalUsable(p)
                              select p).Count() >= 3
                    select def;
            }

            if (!source.TryRandomElement(out PawnKindDef animalDef)) {
                return ir;
            }

            List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned
                where p.kindDef == animalDef && AnimalUsable(p)
                select p).ToList();
            float combatPower = animalDef.combatPower;
            float num = 0f;
            int num2 = 0;
            Pawn pawn = null;
            list.Shuffle();
            foreach (Pawn item in list) {
                if (num + combatPower > adjustedPoints) {
                    break;
                }

                DriveInsane(item);
                num += combatPower;
                num2++;
                pawn = item;
            }

            if (num == 0f) {
                return ir;
            }

            string label;
            string text;
            LetterDef textLetterDef;
            if (num2 == 1) {
                label = "LetterLabelAnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL"));
                text = "AnimalInsanitySingle".Translate(pawn.LabelShort, pawn.Named("ANIMAL"));
                textLetterDef = LetterDefOf.ThreatSmall;
            } else {
                label = "LetterLabelAnimalInsanityMultiple".Translate(animalDef.GetLabelPlural());
                text = "AnimalInsanityMultiple".Translate(animalDef.GetLabelPlural());
                textLetterDef = LetterDefOf.ThreatBig;
            }

            SendLetter(allParams, label, text, textLetterDef, pawn);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(map);
            if (map == Find.CurrentMap) {
                Find.CameraDriver.shaker.DoShake(1f);
            }

            return ir;
        }

        public static bool AnimalUsable(Pawn p) {
            return p.Spawned && !p.Position.Fogged(p.Map) && (!p.InMentalState || !p.MentalStateDef.IsAggro) && !p.Downed && p.Faction == null;
        }

        public static void DriveInsane(Pawn p) {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, null, true);
        }
    }

    public class HumanIncidentParams_AnimalInsanityMass : HumanIncidentParms {
        public Number Points = new Number();
        public string AnimalKind = "";

        public HumanIncidentParams_AnimalInsanityMass() {
        }

        public HumanIncidentParams_AnimalInsanityMass(Target target, HumanLetter letter) : base(target, letter) {
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