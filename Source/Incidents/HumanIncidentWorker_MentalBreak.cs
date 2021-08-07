using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_MentalBreak : HumanIncidentWorker {
        public const String Name = "MentalBreak";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_MentalBreak)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_MentalBreak allParams =
                Tell.AssertNotNull((HumanIncidentParams_MentalBreak) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            foreach (var pawn in allParams.Pawns.Filter(map)) {
                if (pawn.DestroyedOrNull() || pawn.Dead) {
                    continue;
                }

                DefDatabase<MentalBreakDef>.GetNamed(allParams.MentalBreak).Worker.TryStart(pawn, null, false);
            }

            SendLetter(@params);

            return ir;
        }
    }

    public class HumanIncidentParams_MentalBreak : HumanIncidentParams {
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string MentalBreak = "";

        public HumanIncidentParams_MentalBreak() {
        }

        public HumanIncidentParams_MentalBreak(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Pawns: [{Pawns}], MentalBreak: [{MentalBreak}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref MentalBreak, "mentalBreak");
        }
    }
}