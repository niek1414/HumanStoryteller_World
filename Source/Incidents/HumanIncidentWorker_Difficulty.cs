using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Difficulty : HumanIncidentWorker {
        public const String Name = "Difficulty";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Difficulty)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Difficulty
                allParams = Tell.AssertNotNull((HumanIncidentParams_Difficulty) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Find.Storyteller.difficulty = DefDatabase<DifficultyDef>.GetNamed(allParams.Difficulty, false) ?? Find.Storyteller.difficulty;

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_Difficulty : HumanIncidentParms {
        public string Difficulty = "";

        public HumanIncidentParams_Difficulty() {
        }

        public HumanIncidentParams_Difficulty(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Difficulty: [{Difficulty}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Difficulty, "difficulty");
        }
    }
}