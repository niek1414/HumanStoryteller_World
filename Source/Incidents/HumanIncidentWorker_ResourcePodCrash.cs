using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ResourcePodCrash : HumanIncidentWorker {
        public const String Name = "ResourcePodCrash";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ResourcePodCrash)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ResourcePodCrash allParams =
                Tell.AssertNotNull((HumanIncidentParams_ResourcePodCrash) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            List<Thing> things;
            if (allParams.Item != "") {
                int num;
                var paramsAmount = allParams.Amount.GetValue();
                num = paramsAmount != -1 ? Mathf.RoundToInt(paramsAmount) : 20;
                ThingDef droppable = ThingDef.Named(allParams.Item);
                things = new List<Thing>();
                if (droppable.stackLimit <= 0) return ir;
                ThingDef stuff = null;
                if (droppable.MadeFromStuff) {
                    try {
                        if (allParams.Stuff != "") {
                            stuff = (from d in DefDatabase<ThingDef>.AllDefs
                                where d.IsStuff && d.defName.Equals(allParams.Stuff)
                                select d).First();
                        }
                    } catch (InvalidOperationException) {
                    }
                }

                var qc = allParams.ItemQuality != "" ? ItemUtil.GetCategory(allParams.ItemQuality) : QualityCategory.Normal;
                while (num > 0) {
                    var stack = ThingMaker.MakeThing(droppable, stuff);
                    ItemUtil.TrySetQuality(stack,
                        allParams.ItemQuality != "" ? qc : QualityUtility.GenerateQualityRandomEqualChance());
                    var amount = Mathf.Min(stack.def.stackLimit, num);
                    num -= amount;
                    stack.stackCount = amount;
                    stack = ItemUtil.TryMakeMinified(stack);
                    things.Add(stack);
                }
            } else {
                things = ThingSetMakerDefOf.ResourcePod.root.Generate();
            }

            IntVec3 intVec;
            switch (allParams.Location) {
                case "RandomEdge":
                    if (DropCellFinder.TryFindDropSpotNear(CellFinder.RandomEdgeCell(map), map, out var outResult, false, true)) {
                        intVec = outResult;
                    } else {
                        intVec = DropCellFinder.RandomDropSpot(map);
                    }
                    break;
                case "Center":
                    intVec = RCellFinder.TryFindRandomCellNearWith(map.Center, null, map, out var result)
                        ? result
                        : DropCellFinder.RandomDropSpot(map);
                    break;
                default:
                    intVec = DropCellFinder.RandomDropSpot(map);
                    break;
            }

            DropPodUtility.DropThingsNear(intVec, map, things, 110, allParams.InstaPlace, true);
            SendLetter(allParams, "LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent,
                new TargetInfo(intVec, map));

            return ir;
        }
    }

    public class HumanIncidentParams_ResourcePodCrash : HumanIncidentParms {
        public Number Amount;
        public string Item;
        public string ItemQuality;
        public string Stuff;
        public bool InstaPlace;
        public string Location;

        public HumanIncidentParams_ResourcePodCrash() {
        }

        public HumanIncidentParams_ResourcePodCrash(String target, HumanLetter letter, string item = "", string itemQuality = "", string stuff = "",
            bool instaPlace = false, string location = "") :
            this(target, letter, new Number(), item, itemQuality, stuff, instaPlace, location) {
        }

        public HumanIncidentParams_ResourcePodCrash(string target, HumanLetter letter, Number amount, string item, string itemQuality, string stuff,
            bool instaPlace, string location) : base(target, letter) {
            Amount = amount;
            Item = item;
            ItemQuality = itemQuality;
            Stuff = stuff;
            InstaPlace = instaPlace;
            Location = location;
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, Item: {Item}, ItemQuality: {ItemQuality}, Stuff: {Stuff}, InstaPlace: {InstaPlace}, Location: {Location}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Item, "item");
            Scribe_Values.Look(ref ItemQuality, "itemQuality");
            Scribe_Values.Look(ref Stuff, "stuff");
            Scribe_Values.Look(ref InstaPlace, "instaPlace");
            Scribe_Values.Look(ref Location, "location");
        }
    }
}