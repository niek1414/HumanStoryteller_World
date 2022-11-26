using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_Flashstorm : HumanIncidentWorker {
    public const String Name = "Flashstorm";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_Flashstorm)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_Flashstorm
            allParams = Tell.AssertNotNull((HumanIncidentParams_Flashstorm) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        var def = IncidentDef.Named("Flashstorm");
        var number = allParams.Duration.GetValue();
        int duration = Mathf.RoundToInt(number != -1
            ? number * 60000f
            : def.durationDays.RandomInRange * 60000f);
        GameCondition_Flashstorm gameCondition_Flashstorm =
            (GameCondition_Flashstorm) GameConditionMaker.MakeCondition(GameConditionDefOf.Flashstorm, duration);
        map.gameConditionManager.RegisterCondition(gameCondition_Flashstorm);
        SendLetter(allParams, def.letterLabel, def.letterText, def.letterDef,
            new TargetInfo(gameCondition_Flashstorm.centerLocation.ToIntVec3, map));
        if (map.weatherManager.curWeather.rainRate > 0.1f) {
            map.weatherDecider.StartNextWeather();
        }

        return ir;
    }
}

public class HumanIncidentParams_Flashstorm : HumanIncidentParams {
    public Number Duration = new Number();

    public HumanIncidentParams_Flashstorm() {
    }

    public HumanIncidentParams_Flashstorm(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Duration: [{Duration}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref Duration, "duration");
    }
}