using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class ColonistsCheck : CheckCondition {
        public const String Name = "Colonists";

        private DataBankUtil.CompareType _compareType;
        private Number _constant;

        public ColonistsCheck() {
        }

        public ColonistsCheck(DataBankUtil.CompareType compareType, Number constant) {
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constant, nameof(constant), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {           
            return DataBankUtil.CompareValueWithConst(Find.ColonistBar.GetColonistsInOrder().Count, _compareType, _constant.GetValue());
        }

        public override string ToString() {
            return $"CompareType: [{_compareType}], Constant: [{_constant}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Deep.Look(ref _constant, "constant");
        }
    }
}