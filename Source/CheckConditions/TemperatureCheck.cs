using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class TemperatureCheck : CheckCondition {
        public const String Name = "Temperature";

        private DataBank.CompareType _compareType;
        private float _constant;

        public TemperatureCheck() {
        }

        public TemperatureCheck(DataBank.CompareType compareType, float constant) {
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constant, nameof(constant), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Map map = Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).RandomElement();

            return DataBank.CompareValueWithConst(map.mapTemperature.OutdoorTemp, _compareType, _constant);
        }

        public override string ToString() {
            return $"CompareType: {_compareType}, Constant: {_constant}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Values.Look(ref _constant, "constant");
        }
    }
}