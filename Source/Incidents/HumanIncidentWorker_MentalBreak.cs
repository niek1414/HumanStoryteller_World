using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_MentalBreak : HumanIncidentWorker {
        public const String Name = "MentalBreak";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_MentalBreak)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_MentalBreak allParams =
                Tell.AssertNotNull((HumanIncidentParams_MentalBreak) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn == null) {
                    continue;
                }

                DefDatabase<MentalBreakDef>.GetNamed(allParams.MentalBreak).Worker.TryStart(pawn, null, false);
            }

            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_MentalBreak : HumanIncidentParms {
        public List<String> Names;
        public string MentalBreak;

        public HumanIncidentParams_MentalBreak() {
        }

        public HumanIncidentParams_MentalBreak(String target, HumanLetter letter, List<String> names = null, string mentalBreak = "") :
            base(target, letter) {
            Names = names ?? new List<string>();
            MentalBreak = mentalBreak;
        }

        public override string ToString() {
            return $"{base.ToString()}, Names: {Names}, MentalBreak: {MentalBreak}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref MentalBreak, "mentalBreak");
        }
    }
}