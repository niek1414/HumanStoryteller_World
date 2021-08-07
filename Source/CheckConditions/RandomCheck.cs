using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class RandomCheck : CheckCondition {
        public const String Name = "Random";
        private Number _chance;

        public RandomCheck() {
        }

        public RandomCheck(Number chance) {
            _chance = Tell.AssertNotNull(chance, nameof(chance), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return new Random(result.Id + checkPosition).Next(0,100) <= _chance.GetValue();
        }

        public override string ToString() {
            return $"Chance: [{_chance}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref _chance, "chance");
        }
    }
}