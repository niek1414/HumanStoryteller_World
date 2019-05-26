using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class VariableCheck : CheckCondition {
        public const String Name = "Variable";

        private string _variableName;
        private DataBank.CompareType _compareType;
        private Number _constant;

        public VariableCheck() {
        }

        public VariableCheck(string variableName, DataBank.CompareType compareType, Number constantOrVariable) {
            _variableName = Tell.AssertNotNull(variableName, nameof(variableName), GetType().Name);
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constantOrVariable, nameof(constantOrVariable), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return DataBank.CompareVariableWithConst(_variableName, _compareType, _constant.GetValue());
        }

        public override string ToString() {
            return $"VariableName: {_variableName}, CompareType: {_compareType}, Constant: {_constant}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _variableName, "variableName");
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Deep.Look(ref _constant, "constant");
        }
    }
}