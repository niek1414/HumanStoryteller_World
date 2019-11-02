using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TradeRequest : HumanIncidentWorker {
        private static readonly IntRange BaseValueWantedRange = new IntRange(500, 2500);

        private static readonly SimpleCurve ValueWantedFactorFromWealthCurve = new SimpleCurve {
            new CurvePoint(0f, 0.3f),
            new CurvePoint(50000f, 1f),
            new CurvePoint(300000f, 2f)
        };

        private static readonly FloatRange RewardValueFactorRange = new FloatRange(1.5f, 2.1f);

        private static readonly SimpleCurve RewardValueFactorFromWealthCurve = new SimpleCurve {
            new CurvePoint(0f, 1.15f),
            new CurvePoint(50000f, 1f),
            new CurvePoint(300000f, 0.85f)
        };

        private static Dictionary<ThingDef, int> requestCountDict = new Dictionary<ThingDef, int>();

        public const String Name = "TradeRequest";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_TradeRequest)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TradeRequest allParams = Tell.AssertNotNull((HumanIncidentParams_TradeRequest) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            SettlementBase settlementBase = RandomNearbyTradeableSettlement(map.Tile);
            if (settlementBase == null) {
                return ir;
            }

            TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
            int tickDuration;
            var inputDuration = allParams.Duration.GetValue();
            tickDuration = inputDuration != -1 
                ? Mathf.RoundToInt(inputDuration * 60000f) 
                : RandomOfferDurationTicks(map.Tile, settlementBase.Tile);

            component.expiration = Find.TickManager.TicksGame + tickDuration;

            component.requestThingDef = allParams.RequestItem.GetThingDef();
            component.requestCount = Mathf.RoundToInt(allParams.RequestItem.Amount.GetValue());

            if (component.requestThingDef == null) {
                TryFindRandomRequestedThingDef(map, out ThingDef thingDef, out int count);
                component.requestThingDef = thingDef;
                if (component.requestCount == -1) {
                    component.requestCount = count;
                }
            }

            if (component.requestCount == -1) {
                component.requestCount = RandomRequestCount(component.requestThingDef, map);
            }

            component.rewards.ClearAndDestroyContents();
            List<Thing> rewards = allParams.RewardItem.GetThings(true) ?? GenerateRewardsFor(component.requestThingDef, component.requestCount, map);
            component.rewards.TryAddRangeOrTransfer(rewards, true, true);
            
            string text = "LetterCaravanRequest".Translate(settlementBase.Label,
                TradeRequestUtility.RequestedThingLabel(component.requestThingDef, component.requestCount).CapitalizeFirst(),
                (component.requestThingDef.GetStatValueAbstract(StatDefOf.MarketValue) * component.requestCount).ToStringMoney("F0"),
                GenThing.ThingsToCommaList(component.rewards, true).CapitalizeFirst(),
                GenThing.GetMarketValue(component.rewards).ToStringMoney("F0"),
                (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0"),
                CaravanArrivalTimeEstimator.EstimatedTicksToArrive(map.Tile, settlementBase.Tile, null).ToStringTicksToDays("0.#"));
            GenThing.TryAppendSingleRewardInfo(ref text, component.rewards);
            SendLetter(allParams, "LetterLabelCaravanRequest".Translate(), text, LetterDefOf.PositiveEvent, settlementBase, settlementBase.Faction);

            return new IncidentResult_Trade(map.Parent);
        }

        private static List<Thing> GenerateRewardsFor(ThingDef thingDef, int quantity, Map map) {
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.totalMarketValueRange = RewardValueFactorRange * RewardValueFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal) *
                                          thingDef.BaseMarketValue * quantity;
            parms.validator = td => td != thingDef;
            List<Thing> list = null;
            for (int i = 0; i < 10; i++) {
                if (list != null) {
                    for (int j = 0; j < list.Count; j++) {
                        list[j].Destroy();
                    }
                }

                list = ThingSetMakerDefOf.Reward_TradeRequest.root.Generate(parms);
                float num = 0f;
                for (int k = 0; k < list.Count; k++) {
                    num += list[k].MarketValue * list[k].stackCount;
                }

                if (num > thingDef.BaseMarketValue * quantity) {
                    break;
                }
            }

            return list;
        }

        private static void TryFindRandomRequestedThingDef(Map map, out ThingDef thingDef, out int count) {
            requestCountDict.Clear();
            Func<ThingDef, bool> globalValidator = delegate(ThingDef td) {
                if (td.BaseMarketValue / td.BaseMass < 5f) {
                    return false;
                }

                if (!td.alwaysHaulable) {
                    return false;
                }

                CompProperties_Rottable compProperties = td.GetCompProperties<CompProperties_Rottable>();
                if (compProperties != null && compProperties.daysToRotStart < 10f) {
                    return false;
                }

                if (td.ingestible != null && td.ingestible.HumanEdible) {
                    return false;
                }

                if (td == ThingDefOf.Silver) {
                    return false;
                }

                if (!td.PlayerAcquirable) {
                    return false;
                }

                int num = RandomRequestCount(td, map);
                requestCountDict.Add(td, num);
                if (!PlayerItemAccessibilityUtility.PossiblyAccessible(td, num, map)) {
                    return false;
                }

                if (!PlayerItemAccessibilityUtility.PlayerCanMake(td, map)) {
                    return false;
                }

                if (td.thingSetMakerTags != null && td.thingSetMakerTags.Contains("RewardSpecial")) {
                    return false;
                }

                return true;
            };
            if ((from td in ThingSetMakerUtility.allGeneratableItems
                where globalValidator(td)
                select td).TryRandomElement(out thingDef)) {
                count = requestCountDict[thingDef];
                return;
            }

            count = -1;
        }

        private static int RandomRequestCount(ThingDef thingDef, Map map) {
            float randomWantedAmount = BaseValueWantedRange.RandomInRange;
            randomWantedAmount *= ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
            return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(randomWantedAmount / thingDef.BaseMarketValue)));
        }

        private int RandomOfferDurationTicks(int tileIdFrom, int tileIdTo) {
            int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
            float ticksToArrive = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null) / 60000f;
            int travelTime = Mathf.CeilToInt(Mathf.Max(ticksToArrive + 6f, ticksToArrive * 1.35f));
            if (travelTime > SiteTuning.QuestSiteTimeoutDaysRange.max) {
                return -1;
            }

            return 60000 * Mathf.Max(randomInRange, travelTime);
        }

        private static SettlementBase RandomNearbyTradeableSettlement(int originTile) {
            return Find.WorldObjects.SettlementBases.Where(delegate(SettlementBase settlement) {
                if (!settlement.Visitable || settlement.GetComponent<TradeRequestComp>() == null ||
                    settlement.GetComponent<TradeRequestComp>().ActiveRequest) {
                    return false;
                }

                return Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 50f &&
                       Find.WorldReachability.CanReach(originTile, settlement.Tile);
            }).RandomElementByWeightWithFallback(o => Find.WorldGrid.ApproxDistanceInTiles(originTile, o.Tile));
        }
    }

    public class HumanIncidentParams_TradeRequest : HumanIncidentParms {
        public Number Points = new Number();
        public Number Duration = new Number();

        public Item RequestItem = new Item("", "", "", new Number(5));
        public Item RewardItem = new Item("", "", "", new Number(5));

        public HumanIncidentParams_TradeRequest() {
        }

        public HumanIncidentParams_TradeRequest(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return
                $"{base.ToString()}, Points: [{Points}], Duration: [{Duration}], RequestItem: [{RequestItem}], RewardItem: [{RewardItem}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Deep.Look(ref RequestItem, "requestItem");
            Scribe_Deep.Look(ref RewardItem, "rewardItem");
        }
    }
}