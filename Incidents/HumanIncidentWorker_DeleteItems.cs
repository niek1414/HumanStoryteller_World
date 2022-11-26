using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_DeleteItems : HumanIncidentWorker {
    public const String Name = "DeleteItems";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_DeleteItems)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_DeleteItems allParams =
            Tell.AssertNotNull((HumanIncidentParams_DeleteItems) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        if (allParams.Item != "") {
            ThingDef removeDef = ThingDef.Named(allParams.Item);
            RemoveThingsOfType(removeDef, Mathf.RoundToInt(allParams.Amount.GetValue()), map);
        }

        SendLetter(@params);

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

public class HumanIncidentParams_DeleteItems : HumanIncidentParams {
    public Number Amount = new Number(1);
    public string Item = "";

    public HumanIncidentParams_DeleteItems() {
    }

    public HumanIncidentParams_DeleteItems(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Amount: [{Amount}], Item: [{Item}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Amount, "amount");
        Scribe_Values.Look(ref Item, "item");
    }
}