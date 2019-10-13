using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_AnimalInsanitySingle : HumanIncidentWorker {
        public const String Name = "AnimalInsanitySingle";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_AnimalInsanitySingle)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_AnimalInsanitySingle allParams =
                Tell.AssertNotNull((HumanIncidentParams_AnimalInsanitySingle) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            Pawn firstTarget = null;
            foreach (var target in allParams.Names.Filter(map)) {
                firstTarget = target;
                if (target.DestroyedOrNull() || target.Downed || target.Dead || !target.SpawnedOrAnyParentSpawned) continue;
                HumanIncidentWorker_AnimalInsanityMass.DriveInsane(target);
            }
            if (firstTarget == null){
                if (!TryFindRandomAnimal(map, out Pawn animal)) {
                    return ir;
                }

                firstTarget = animal;
                HumanIncidentWorker_AnimalInsanityMass.DriveInsane(animal);
            }

            var text = "AnimalInsanitySingle".Translate(firstTarget.Label, firstTarget.Named("ANIMAL"));
            var title = "LetterLabelAnimalInsanitySingle".Translate(firstTarget.Label, firstTarget.Named("ANIMAL"));
            SendLetter(allParams, title, text, LetterDefOf.ThreatSmall, firstTarget);
            return ir;
        }

        private bool TryFindRandomAnimal(Map map, out Pawn animal) {
            int maxPoints = 150;
            if (GenDate.DaysPassed < 7) {
                maxPoints = 40;
            }

            return (from p in map.mapPawns.AllPawnsSpawned
                where p.RaceProps.Animal && p.kindDef.combatPower <= (float) maxPoints && IncidentWorker_AnimalInsanityMass.AnimalUsable(p)
                select p).TryRandomElement(out animal);
        }
    }

    public class HumanIncidentParams_AnimalInsanitySingle : HumanIncidentParms {
        public PawnGroupSelector Names;

        public HumanIncidentParams_AnimalInsanitySingle() {
        }

        public HumanIncidentParams_AnimalInsanitySingle(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Names: {Names}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Names, "names");
        }
    }
}