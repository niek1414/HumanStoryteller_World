using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class ItemMapCheck : CheckCondition {
        public const String Name = "ItemMap";

        private ThingDef _item;
        private DataBankUtil.CompareType _compareType;
        private Number _constant;

        public ItemMapCheck() {
        }

        public ItemMapCheck(ThingDef item, DataBankUtil.CompareType compareType, Number constant) {
            _item = Tell.AssertNotNull(item, nameof(item), GetType().Name);
            _compareType = Tell.AssertNotNull(compareType, nameof(compareType), GetType().Name);
            _constant = Tell.AssertNotNull(constant, nameof(constant), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            int count = result.Target.GetMapFromTarget().listerThings.ThingsMatching(new ThingRequest {
                singleDef = _item,
                group = ThingRequestGroup.HaulableEverOrMinifiable
            }).Count;
            return DataBankUtil.CompareValueWithConst(count, _compareType, _constant.GetValue());
        }

        public override string ToString() {
            return $"Faction: {_item}, CompareType: {_compareType}, Constant: {_constant}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Defs.Look(ref _item, "item");
            Scribe_Values.Look(ref _compareType, "compareType");
            Scribe_Deep.Look(ref _constant, "constant");
        }
    }
}