using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class DifficultyCheck : CheckCondition {
        public const String Name = "Difficulty";
        private DifficultyDef _difficulty;

        public DifficultyCheck() {
        }

        public DifficultyCheck(DifficultyDef difficulty) {
            _difficulty = Tell.AssertNotNull(difficulty, nameof(difficulty), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return Find.Storyteller.difficulty.defName.Equals(_difficulty.defName);
        }

        public override string ToString() {
            return $"Difficulty: {_difficulty}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Defs.Look(ref _difficulty, "difficulty");
        }
    }
}