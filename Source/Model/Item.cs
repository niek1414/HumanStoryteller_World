using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Model {
    public class Item : IExposable {
        public string Thing = "";
        public string Stuff = "";
        public string Quality = "";
        public Number Amount = new Number();

        public Item() {
        }

        public Item(string thing, string stuff, string quality, Number amount) {
            Thing = thing;
            Stuff = stuff;
            Quality = quality;
            Amount = amount;
        }

        public bool NotEmpty() {
            return Thing != "";
        }

        public override string ToString() {
            return $"Thing: [{Thing}], Stuff: [{Stuff}], Quality: [{Quality}], Amount: [{Amount}]";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Thing, "thing");
            Scribe_Values.Look(ref Stuff, "stuff");
            Scribe_Values.Look(ref Quality, "quality");
            Scribe_Deep.Look(ref Amount, "amount");
        }

        public QualityCategory GetQualityWithFallback(QualityCategory fallback) {
            return ItemUtil.GetCategory(Quality, fallback);
        }

        public ThingDef GetThingDef() {
            return (from def in DefDatabase<ThingDef>.AllDefs
                where def.defName.Equals(Thing)
                select def).RandomElementWithFallback();
        }

        public ThingDef GetStuffDef() {
            return (from d in DefDatabase<ThingDef>.AllDefs
                where d.IsStuff && d.defName.Equals(Stuff)
                select d).FirstOrFallback();
        }

        public ThingWithComps GetThing() {
            var thingDef = GetThingDef();
            if (thingDef == null) {
                return null;
            }

            var stuff = GetStuffDef() ?? GenStuff.RandomStuffFor(thingDef);
            if (stuff == null && thingDef.MadeFromStuff) {
                return null;
            }

            var thing = (ThingWithComps) ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? stuff : null);
            if (thing == null) {
                return null;
            }

            var quality = GetQualityWithFallback(QualityCategory.Normal);
            ItemUtil.TrySetQuality(thing, quality);
            return thing;
        }

        public List<Thing> GetThings(bool returnNullIfEmpty = false) {
            List<Thing> GetThings() {
                List<Thing> things = new List<Thing>();
                
                int num = Mathf.RoundToInt(Amount.GetValue());
                if (num == -1) {
                    num = 1;
                }

                var thingDef = GetThingDef();
                if (thingDef == null) {
                    return things;
                }

                if (thingDef.stackLimit <= 0) return things;

                var stuff = GetStuffDef() ?? GenStuff.RandomStuffFor(thingDef);
                if (stuff == null && thingDef.MadeFromStuff) {
                    return things;
                }

                var quality = GetQualityWithFallback(QualityCategory.Normal);
                while (num > 0) {
                    var stack = ThingMaker.MakeThing(thingDef, thingDef.MadeFromStuff ? stuff : null);
                    ItemUtil.TrySetQuality(stack, quality);
                    var amount = Mathf.Min(stack.def.stackLimit, num);
                    num -= amount;
                    stack.stackCount = amount;
                    stack = ItemUtil.TryMakeMinified(stack);
                    things.Add(stack);
                }

                return things;
            }

            var result = GetThings();
            if (result.NullOrEmpty() && returnNullIfEmpty) {
                return null;
            }

            return result;
        }
    }
}