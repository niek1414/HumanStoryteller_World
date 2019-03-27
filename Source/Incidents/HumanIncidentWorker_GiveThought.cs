using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_GiveThought : HumanIncidentWorker {
        public const String Name = "GiveThought";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_GiveThought)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_GiveThought allParams =
                Tell.AssertNotNull((HumanIncidentParams_GiveThought) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn?.needs?.mood?.thoughts?.memories == null) {
                    continue;//Not found or animal/wild man
                }
                ThoughtDef def = allParams.ThoughtType == "" || allParams.ThoughtType == "custom" ? null : DefDatabase<ThoughtDef>.GetNamed(allParams.ThoughtType, false);
                Thought_Memory thought;
                if (def == null) {
                    thought = (Thought_Memory) ThoughtMaker.MakeThought(ThoughtDefOf.DebugGood);
                    thought.CurStage.label = allParams.ThoughtLabel;
                    thought.CurStage.description = allParams.ThoughtDescription;
                    thought.CurStage.baseMoodEffect = allParams.ThoughtEffect;
                    thought.def.durationDays = allParams.ThoughtDuration;
                } else {
                    thought = (Thought_Memory) ThoughtMaker.MakeThought(def);
                    thought.otherPawn = PawnUtil.GetPawnByName(allParams.OtherPawn);
                }

                pawn.needs.mood.thoughts.memories.TryGainMemory(thought);
            }

            if (parms.Letter?.Type != null) {
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_GiveThought : HumanIncidentParms {
        public List<String> Names;
        public string ThoughtType;
        public string ThoughtLabel;
        public string ThoughtDescription;
        public float ThoughtEffect;
        public float ThoughtDuration;
        public string OtherPawn;

        public HumanIncidentParams_GiveThought() {
        }

        public HumanIncidentParams_GiveThought(String target, HumanLetter letter,
            List<String> names = null,
            string thoughtType = "",
            string thoughtLabel = "",
            string thoughtDescription = "",
            float thoughtEffect = 0,
            float thoughtDuration = 1,
            string otherPawn = "") :
            base(target, letter) {
            Names = names ?? new List<string>();
            ThoughtType = thoughtType;
            ThoughtLabel = thoughtLabel;
            ThoughtDescription = thoughtDescription;
            ThoughtEffect = thoughtEffect;
            ThoughtDuration = thoughtDuration;
            OtherPawn = otherPawn;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref ThoughtType, "thoughtType");
            Scribe_Values.Look(ref ThoughtLabel, "thoughtLabel");
            Scribe_Values.Look(ref ThoughtDescription, "thoughtDescription");
            Scribe_Values.Look(ref ThoughtEffect, "thoughtEffect");
            Scribe_Values.Look(ref ThoughtDuration, "thoughtDuration");
            Scribe_Values.Look(ref OtherPawn, "otherPawn");
        }
    }
}