using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_DeleteItems : HumanIncidentWorker {
        public const String Name = "DeleteItems";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_DeleteItems)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_DeleteItems allParams =
                Tell.AssertNotNull((HumanIncidentParams_DeleteItems) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            if (allParams.Item != "") {
                int num;
                var paramsAmount = allParams.Amount.GetValue();
                num = paramsAmount != -1 ? Mathf.RoundToInt(paramsAmount) : 1;
                ThingDef removeDef = ThingDef.Named(allParams.Item);

                RemoveThingsOfType(removeDef, num, map);
            }

            SendLetter(parms);

            return ir;
        }

        public static void RemoveThingsOfType(ThingDef resDef, int debt, Map map) {
            var list = map.listerThings.ThingsMatching(new ThingRequest {
                singleDef = resDef,
                group = ThingRequestGroup.HaulableEverOrMinifiable
            });
            var index = 0;
            while (debt > 0) {
                Thing toGive;
                if (list.Count > index) {
                    toGive = list[index];
                    index++;
                } else {
                    break;
                }

                int num = Math.Min(debt, toGive.stackCount);
                toGive.SplitOff(num).Destroy();
                debt -= num;
            }
        }
    }

    public class HumanIncidentParams_DeleteItems : HumanIncidentParms {
        public Number Amount;
        public string Item;

        public HumanIncidentParams_DeleteItems() {
        }

        public HumanIncidentParams_DeleteItems(String target, HumanLetter letter, string item = "") :
            this(target, letter, new Number(), item) {
        }

        public HumanIncidentParams_DeleteItems(string target, HumanLetter letter, Number amount, string item) : base(target, letter) {
            Amount = amount;
            Item = item;
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, Item: {Item}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Item, "item");
        }
    }
}