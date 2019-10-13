using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_MentalBreak : HumanIncidentWorker {
        public const String Name = "MentalBreak";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_MentalBreak)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_MentalBreak allParams =
                Tell.AssertNotNull((HumanIncidentParams_MentalBreak) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            foreach (var pawn in allParams.Names.Filter(map)) {
                if (pawn.DestroyedOrNull() || pawn.Dead) {
                    continue;
                }

                DefDatabase<MentalBreakDef>.GetNamed(allParams.MentalBreak).Worker.TryStart(pawn, null, false);
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_MentalBreak : HumanIncidentParms {
        public PawnGroupSelector Names = new PawnGroupSelector();
        public string MentalBreak = "";

        public HumanIncidentParams_MentalBreak() {
        }

        public HumanIncidentParams_MentalBreak(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Names: {Names}, MentalBreak: {MentalBreak}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Names, "names");
            Scribe_Values.Look(ref MentalBreak, "mentalBreak");
        }
    }
}