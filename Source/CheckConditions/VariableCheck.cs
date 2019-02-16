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
        private float _constant;

        public VariableCheck() {
        }

        public VariableCheck(string variableName, DataBank.CompareType compareType, float constant) {
            _variableName = variableName;
            _compareType = compareType;
            _constant = constant;
        }

        public override bool Check(IncidentResult result) {
            return DataBank.CompareVariableWithConst(_variableName, _compareType, _constant);
        }

        public override string ToString() {
            return $"VariableName: {_variableName}, CompareType: {_compareType}, Constant: {_constant}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _variableName, "variableName");
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Values.Look(ref _constant, "constant");
        }
    }
}