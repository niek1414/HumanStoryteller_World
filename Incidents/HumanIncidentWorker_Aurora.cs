using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Aurora : HumanIncidentWorker {
    public const String Name = "Aurora";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_Aurora)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_Aurora
            allParams = Tell.AssertNotNull((HumanIncidentParams_Aurora) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        var def = IncidentDef.Named("Aurora");
        var parmDuration = allParams.Duration.GetValue();
        int duration = Mathf.RoundToInt(parmDuration != -1
            ? parmDuration * 60000f
            : def.durationDays.RandomInRange * 60000f);
        GameCondition_Aurora gameCondition_aurora =
            (GameCondition_Aurora) GameConditionMaker.MakeCondition(GameConditionDefOf.Aurora, duration);
        map.gameConditionManager.RegisterCondition(gameCondition_aurora);
        SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef, null);
        return ir;
    }
}

public class HumanIncidentParams_Aurora : HumanIncidentParams {
    public Number Duration = new Number();

    public HumanIncidentParams_Aurora() {
    }

    public HumanIncidentParams_Aurora(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Duration: [{Duration}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Duration, "duration");
    }
}