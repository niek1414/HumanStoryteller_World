using System;
using System.Collections.Generic;
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
                num = allParams.Amount != -1 ? Mathf.RoundToInt(allParams.Amount) : 20;
                ThingDef droppable = ThingDef.Named(allParams.Item);
                things = new List<Thing>();
                if (droppable.stackLimit <= 0) return ir;
                var qc = allParams.ItemQuality != "" ? GetCategory(allParams.ItemQuality) : QualityCategory.Normal;
                while (num > 0) {
                    var stack = ThingMaker.MakeThing(droppable);
                    TrySetQuality(stack,
                        allParams.ItemQuality != "" ? qc : QualityUtility.GenerateQualityRandomEqualChance());
                    var amount = Mathf.Min(stack.def.stackLimit, num);
                    num -= amount;
                    stack.stackCount = amount;
                    things.Add(stack);
                }
            } else {
                things = ThingSetMakerDefOf.ResourcePod.root.Generate();
            }

            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true);

            SendLetter(allParams, "LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent,
                new TargetInfo(intVec, map));

            return ir;
        }

        private static QualityCategory GetCategory(string q) {
            switch (q) {
                case "Awful":
                    return QualityCategory.Awful;
                case "Poor":
                    return QualityCategory.Poor;
                case "Normal":
                    return QualityCategory.Normal;
                case "Good":
                    return QualityCategory.Good;
                case "Excellent":
                    return QualityCategory.Excellent;
                case "Masterwork ":
                    return QualityCategory.Masterwork;
                default:
                    return QualityUtility.GenerateQualityRandomEqualChance();
            }
        }

        private static void TrySetQuality(Thing t, QualityCategory qc) {
            CompQuality compQuality = t is MinifiedThing minifiedThing
                ? minifiedThing.InnerThing.TryGetComp<CompQuality>()
                : t.TryGetComp<CompQuality>();
            compQuality?.SetQuality(qc, ArtGenerationContext.Colony);
        }
    }

    public class HumanIncidentParams_ResourcePodCrash : HumanIncidentParms {
        public float Amount;
        public string Item;
        public string ItemQuality;

        public HumanIncidentParams_ResourcePodCrash() {
        }

        public HumanIncidentParams_ResourcePodCrash(String target, HumanLetter letter, float amount = -1, string item = "", string itemQuality = "") :
            base(target, letter) {
            Amount = amount;
            Item = item;
            ItemQuality = itemQuality;
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, Item: {Item}, Quality: {ItemQuality}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Item, "item");
            Scribe_Values.Look(ref ItemQuality, "itemQuality");
        }
    }
}