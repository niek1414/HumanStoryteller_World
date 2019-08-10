using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
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
            Pawn target = null;
            if (allParams.Names.Count > 0) {
                foreach (var name in allParams.Names) {
                    target = PawnUtil.GetPawnByName(name);
                    if (target == null) continue;
                    HumanIncidentWorker_AnimalInsanityMass.DriveInsane(target);
                }
            } 
            if (target == null){
                if (!TryFindRandomAnimal(map, out Pawn animal)) {
                    return ir;
                }

                target = animal;
                HumanIncidentWorker_AnimalInsanityMass.DriveInsane(animal);
            }

            var text = "AnimalInsanitySingle".Translate(target.Label, target.Named("ANIMAL"));
            var title = "LetterLabelAnimalInsanitySingle".Translate(target.Label, target.Named("ANIMAL"));
            SendLetter(allParams, title, text, LetterDefOf.ThreatSmall, target);
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
        public List<String> Names = new List<string>();

        public HumanIncidentParams_AnimalInsanitySingle() {
        }

        public HumanIncidentParams_AnimalInsanitySingle(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
        }
    }
}