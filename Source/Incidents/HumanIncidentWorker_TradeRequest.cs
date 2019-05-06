using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
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
            if (inputDuration != -1) {
                tickDuration = Mathf.RoundToInt(inputDuration * 60000f);
            } else {
                tickDuration = RandomOfferDurationTicks(map.Tile, settlementBase.Tile);
            }

            component.expiration = Find.TickManager.TicksGame + tickDuration;

            component.requestThingDef = null;
            component.requestCount = -1;
            if (allParams.RequestItem != "") {
                component.requestThingDef = ThingDef.Named(allParams.RequestItem);
            }

            component.requestCount = Mathf.RoundToInt(allParams.RequestAmount.GetValue());
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
            List<Thing> rewards;
            if (allParams.RewardItem == "") {
                rewards = GenerateRewardsFor(component.requestThingDef, component.requestCount, map);
            } else {
                rewards = GenerateRewards(allParams.RewardAmount, allParams.RewardItem, allParams.RewardStuff, allParams.RewardItemQuality);
            }

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

        private List<Thing> GenerateRewards(Number number, string item, string stuff, string quality) {
            List<Thing> things = new List<Thing>();
            int num = Mathf.RoundToInt(number.GetValue());
            ThingDef droppable = ThingDef.Named(item);
            if (droppable.stackLimit <= 0) return things;
            ThingDef stuffDef = null;
            if (droppable.MadeFromStuff) {
                try {
                    if (stuff != "") {
                        stuffDef = (from d in DefDatabase<ThingDef>.AllDefs
                            where d.IsStuff && d.defName.Equals(stuff)
                            select d).First();
                    }
                } catch (InvalidOperationException) {
                }
            }

            var qc = quality != "" ? ItemUtil.GetCategory(quality) : QualityCategory.Normal;
            while (num > 0) {
                var stack = ThingMaker.MakeThing(droppable, stuffDef);
                ItemUtil.TrySetQuality(stack,
                    quality != "" ? qc : QualityUtility.GenerateQualityRandomEqualChance());
                var amount = Mathf.Min(stack.def.stackLimit, num);
                num -= amount;
                stack.stackCount = amount;
                stack = ItemUtil.TryMakeMinified(stack);
                things.Add(stack);
            }

            return things;
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
            float num = BaseValueWantedRange.RandomInRange;
            num *= ValueWantedFactorFromWealthCurve.Evaluate(map.wealthWatcher.WealthTotal);
            return ThingUtility.RoundedResourceStackCount(Mathf.Max(1, Mathf.RoundToInt(num / thingDef.BaseMarketValue)));
        }

        private int RandomOfferDurationTicks(int tileIdFrom, int tileIdTo) {
            int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
            int num = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
            float num2 = num / 60000f;
            int num3 = Mathf.CeilToInt(Mathf.Max(num2 + 6f, num2 * 1.35f));
            int num4 = num3;
            IntRange questSiteTimeoutDaysRange = SiteTuning.QuestSiteTimeoutDaysRange;
            if (num4 > questSiteTimeoutDaysRange.max) {
                return -1;
            }

            int num5 = Mathf.Max(randomInRange, num3);
            return 60000 * num5;
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
        public Number Points;
        public Number Duration;

        public Number RequestAmount;
        public string RequestItem;

        public Number RewardAmount;
        public string RewardItem;
        public string RewardItemQuality;
        public string RewardStuff;

        public HumanIncidentParams_TradeRequest() {
        }

        public HumanIncidentParams_TradeRequest(string target, HumanLetter letter,
            Number duration,
            Number requestAmount,
            string requestItem,
            Number rewardAmount,
            string rewardItem,
            string rewardItemQuality,
            string rewardStuff) : base(target, letter) {
            Duration = duration;
            RequestAmount = requestAmount;
            RequestItem = requestItem;
            RewardAmount = rewardAmount;
            RewardItem = rewardItem;
            RewardItemQuality = rewardItemQuality;
            RewardStuff = rewardStuff;
        }


        public HumanIncidentParams_TradeRequest(string target, HumanLetter letter,
            string requestItem = "",
            string rewardItem = "",
            string rewardItemQuality = "",
            string rewardStuff = "") : this(target, letter,
            new Number(),
            new Number(),
            requestItem,
            new Number(5),
            rewardItem,
            rewardItemQuality,
            rewardStuff) {
        }

        public override string ToString() {
            return
                $"{base.ToString()}, Points: {Points}, Duration: {Duration}, RequestAmount: {RequestAmount}, RequestItem: {RequestItem}, RewardAmount: {RewardAmount}, RewardItem: {RewardItem}, RewardItemQuality: {RewardItemQuality}, RewardStuff: {RewardStuff}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Deep.Look(ref RequestAmount, "requestAmount");
            Scribe_Values.Look(ref RequestItem, "requestItem");
            Scribe_Deep.Look(ref RewardAmount, "rewardAmount");
            Scribe_Values.Look(ref RewardItem, "rewardItem");
            Scribe_Values.Look(ref RewardItemQuality, "rewardItemQuality");
            Scribe_Values.Look(ref RewardStuff, "rewardStuff");
        }
    }
}