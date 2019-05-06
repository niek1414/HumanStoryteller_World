using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_HealPawn : HumanIncidentWorker {
        public const String Name = "HealPawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_HealPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_HealPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_HealPawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn == null) {
                    continue;
                }

                IEnumerable<Hediff_Injury> injuries;
                if (allParams.Miracle) {
                    injuries = from x in pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                        select x;
                } else {
                    injuries = from x in pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
                        where x.CanHealNaturally() || x.CanHealFromTending()
                        select x;
                }
                foreach (var injury in injuries) {
                    injury.Heal(injury.Severity);
                }
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_HealPawn : HumanIncidentParms {
        public List<String> Names;
        public bool Miracle;

        public HumanIncidentParams_HealPawn() {
        }

        public HumanIncidentParams_HealPawn(String target, HumanLetter letter, List<String> names = null, bool miracle = false) :
            base(target, letter) {
            Names = names ?? new List<string>();
            Miracle = miracle;
        }

        public override string ToString() {
            return $"{base.ToString()}, Names: {Names}, Miracle: {Miracle}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref Miracle, "miracle");
        }
    }
}