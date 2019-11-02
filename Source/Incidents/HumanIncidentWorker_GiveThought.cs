using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_GiveThought : HumanIncidentWorker {
        public const String Name = "GiveThought";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_GiveThought)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_GiveThought allParams =
                Tell.AssertNotNull((HumanIncidentParams_GiveThought) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            foreach (var pawn in allParams.Pawns.Filter(map)) {
                if (pawn?.needs?.mood?.thoughts?.memories == null) {
                    continue; //Not found or animal/wild man
                }

                ThoughtDef def = allParams.ThoughtType == "" || allParams.ThoughtType == "custom"
                    ? null
                    : DefDatabase<ThoughtDef>.GetNamed(allParams.ThoughtType, false);
                Thought_Memory thought;
                if (def == null) {
                    thought = (Thought_Memory) ThoughtMaker.MakeThought(ThoughtDefOf.DebugGood);
                    thought.CurStage.label = allParams.ThoughtLabel;
                    thought.CurStage.description = allParams.ThoughtDescription;
                    thought.CurStage.baseMoodEffect = allParams.ThoughtEffect.GetValue();
                    thought.def.durationDays = allParams.ThoughtDuration.GetValue();
                } else {
                    thought = (Thought_Memory) ThoughtMaker.MakeThought(def);
                    thought.otherPawn = PawnUtil.GetPawnByName(allParams.OtherPawn);
                }

                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_GiveThought : HumanIncidentParms {
        public Number ThoughtEffect = new Number(0);
        public Number ThoughtDuration = new Number(1);
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string ThoughtType = "";
        public string ThoughtLabel = "";
        public string ThoughtDescription = "";
        public string OtherPawn = "";

        public HumanIncidentParams_GiveThought() {
        }

        public HumanIncidentParams_GiveThought(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref ThoughtEffect, "thoughtEffect");
            Scribe_Deep.Look(ref ThoughtDuration, "thoughtDuration");
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref ThoughtType, "thoughtType");
            Scribe_Values.Look(ref ThoughtLabel, "thoughtLabel");
            Scribe_Values.Look(ref ThoughtDescription, "thoughtDescription");
            Scribe_Values.Look(ref OtherPawn, "otherPawn");
        }
    }
}