using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class RandomCheck : CheckCondition {
        public const String Name = "Random";
        private float _chance;

        public RandomCheck() {
        }

        public RandomCheck(float chance) {
            _chance = Tell.AssertNotNull(chance, nameof(chance), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return new Random(result.Id + checkPosition).Next(0,100) <= _chance;
        }

        public override string ToString() {
            return $"Chance: {_chance}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _chance, "chance");
        }
    }
}