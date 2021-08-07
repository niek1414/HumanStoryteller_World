using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class GroupCountCheck : CheckCondition {
        public const String Name = "GroupCount";

        private string _group;
        private DataBankUtil.CompareType _compareType;
        private Number _constant;

        public GroupCountCheck() {
        }

        public GroupCountCheck(string group, DataBankUtil.CompareType compareType, Number constant) {
            _group = Tell.AssertNotNull(group, nameof(group), GetType().Name);
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constant, nameof(constant), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            int count = PawnGroupUtil.GetGroupByName(_group)?.Pawns.Count ?? 0;
            return DataBankUtil.CompareValueWithConst(count, _compareType, _constant.GetValue());
        }

        public override string ToString() {
            return $"Group: [{_group}], CompareType: [{_compareType}], Constant: [{_constant}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _group, "group");
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Deep.Look(ref _constant, "constant");
        }
    }
}