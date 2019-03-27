using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class RelationCheck : CheckCondition {
        public const String Name = "Relation";

        private Faction _faction;
        private DataBank.CompareType _compareType;
        private float _constant;

        public RelationCheck() {
        }

        public RelationCheck(Faction faction, DataBank.CompareType compareType, float constant) {
            _faction = Tell.AssertNotNull(faction, nameof(faction), GetType().Name);
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constant, nameof(constant), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return DataBank.CompareValueWithConst(_faction.PlayerGoodwill, _compareType, _constant);
        }

        public override string ToString() {
            return $"Faction: {_faction}, CompareType: {_compareType}, Constant: {_constant}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref _faction, "faction");
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Values.Look(ref _constant, "constant");
        }
    }
}