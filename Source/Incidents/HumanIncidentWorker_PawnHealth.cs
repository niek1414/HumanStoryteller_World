using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_PawnHealth : HumanIncidentWorker {
        public const String Name = "PawnHealth";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PawnHealth)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PawnHealth allParams =
                Tell.AssertNotNull((HumanIncidentParams_PawnHealth) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn == null) {
                    continue;
                }

                switch (allParams.HealthAction) {
                    case "Anesthetize":
                        HealthUtility.TryAnesthetize(pawn);
                        break;
                    case "DamageTillDowned":
                        HealthUtility.DamageUntilDowned(pawn);
                        break;
                    case "Immobilize":
                        HealthUtility.DamageLegsUntilIncapableOfMoving(pawn);
                        break;
                    case "PsychicShock":
                        HediffGiverUtility.TryApply(pawn, HediffDefOf.PsychicShock, null);
                        break;
                    case "MissingBodyPart":
                        HediffGiverUtility.TryApply(pawn, HediffDefOf.MissingBodyPart, allParams.BodyPart != ""? new List<BodyPartDef>{DefDatabase<BodyPartDef>.GetNamed(allParams.BodyPart, false)} : pawn.health.hediffSet.GetNotMissingParts().Select(o => o.def).ToList());
                        break;
                    case "StabBodyPart":
                        HediffGiverUtility.TryApply(pawn, HediffDefOf.Stab, allParams.BodyPart != ""? new List<BodyPartDef>{DefDatabase<BodyPartDef>.GetNamed(allParams.BodyPart, false)} : pawn.health.hediffSet.GetNotMissingParts().Select(o => o.def).ToList());
                        break;
                }
            }

            if (parms.Letter?.Type != null) {
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_PawnHealth : HumanIncidentParms {
        public List<String> Names;
        public string HealthAction;
        public string BodyPart;

        public HumanIncidentParams_PawnHealth() {
        }

        public HumanIncidentParams_PawnHealth(String target, HumanLetter letter, List<String> names = null, string healthAction = "", string bodyPart = "") :
            base(target, letter) {
            Names = names ?? new List<string>();
            HealthAction = healthAction;
            BodyPart = bodyPart;
        }


        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref HealthAction, "healthAction");
            Scribe_Values.Look(ref BodyPart, "bodyPart");
        }
    }
}