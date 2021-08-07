using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class OnLocationCheck : CheckCondition {
        public const String Name = "OnLocation";
        private Location _location;
        private FilterCategory _filterCategory;
        private Number _atLeastAmount;

        private PawnGroupSelector _pawnGroup;
        private ThingDef _item;
        private List<ThingRequestGroup> _thingRequestGroups;

        public OnLocationCheck() {
        }

        private OnLocationCheck(Location location, FilterCategory filterCategory, Number atLeastAmount) {
            _location = Tell.AssertNotNull(location, nameof(location), GetType().Name);
            _filterCategory = Tell.AssertNotNull(filterCategory, nameof(filterCategory), GetType().Name);
            _atLeastAmount = Tell.AssertNotNull(atLeastAmount, nameof(atLeastAmount), GetType().Name);
        }

        public OnLocationCheck(Location location, PawnGroupSelector pawnGroup, Number atLeastAmount) : this(location, FilterCategory.Pawn,
            atLeastAmount) {
            _pawnGroup = Tell.AssertNotNull(pawnGroup, nameof(pawnGroup), GetType().Name);
        }

        public OnLocationCheck(Location location, ThingDef item, Number atLeastAmount) : this(location, FilterCategory.Item, atLeastAmount) {
            _item = Tell.AssertNotNull(item, nameof(item), GetType().Name);
        }

        public OnLocationCheck(Location location, List<ThingRequestGroup> thingRequestGroups, Number atLeastAmount) : this(location,
            FilterCategory.Category,
            atLeastAmount) {
            _thingRequestGroups = Tell.AssertNotNull(thingRequestGroups, nameof(thingRequestGroups), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Map map = result.Target.GetMapFromTarget();

            var foundCount = 0;
            var neededCount = _atLeastAmount.GetValue();
            neededCount = neededCount == -1 ? 1 : neededCount;
            if (neededCount == 0) {
                // Its always at least 0...
                Tell.Warn("OnLocation check returned true because the 'atLeastAmount' was 0");
                return true;
            }

            var locationZone = _location.GetZone(map);
            switch (_filterCategory) {
                case FilterCategory.Pawn:
                    if (_pawnGroup.FilterEnumerable(map)
                        .Where(pawn => !pawn.DestroyedOrNull() && pawn.Map == map && locationZone.Contains(pawn.Position))
                        .Any(pawn => ++foundCount >= neededCount)) {
                        return true;
                    }

                    break;
                case FilterCategory.Item:
                    if (map.listerThings.ThingsOfDef(_item).Where(thing => locationZone.Contains(thing.Position))
                        .Any(pawn => ++foundCount >= neededCount)) {
                        return true;
                    }

                    break;
                case FilterCategory.Category:
                    foreach (var thingRequestGroup in _thingRequestGroups) {
                        if (map.listerThings.ThingsInGroup(thingRequestGroup).Where(thing => locationZone.Contains(thing.Position))
                            .Any(pawn => ++foundCount >= neededCount)) {
                            return true;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown filter category: " + _filterCategory);
            }

            Tell.Log("Not found enough during 'OnLocation' check. Amount found: " + foundCount + " but needed: " + neededCount);
            return false;
        }

        public override string ToString() {
            return $"Location: [{_location}], FilterCategory: [{_filterCategory}], AtLeastAmount: [{_atLeastAmount}], PawnGroup: [{_pawnGroup}], Item: [{_item}], ThingRequestGroups: [{_thingRequestGroups.ToStringSafeEnumerable()}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref _location, "location");
            Scribe_Values.Look(ref _filterCategory, "filterCategory");
            Scribe_Deep.Look(ref _atLeastAmount, "atLeastAmount");
            
            Scribe_Deep.Look(ref _pawnGroup, "pawnGroup");
            Scribe_Defs.Look(ref _item, "item");
            Scribe_Collections.Look(ref _thingRequestGroups, "thingRequestGroups", LookMode.Value);
        }
    }

    public enum FilterCategory {
        Pawn,
        Item,
        Category
    }
}