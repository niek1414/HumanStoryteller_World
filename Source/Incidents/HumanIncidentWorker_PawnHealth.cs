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
    class HumanIncidentWorker_PawnHealth : HumanIncidentWorker {
        public const String Name = "PawnHealth";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_PawnHealth)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_PawnHealth allParams =
                Tell.AssertNotNull((HumanIncidentParams_PawnHealth) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            foreach (var pawn in allParams.Pawns.Filter(map)) {
                if (pawn.DestroyedOrNull()) {
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

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_PawnHealth : HumanIncidentParms {
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string HealthAction = "";
        public string BodyPart = "";

        public HumanIncidentParams_PawnHealth() {
        }

        public HumanIncidentParams_PawnHealth(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Pawns: [{Pawns}], HealthAction: [{HealthAction}], BodyPart: [{BodyPart}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref HealthAction, "healthAction");
            Scribe_Values.Look(ref BodyPart, "bodyPart");
        }
    }
}