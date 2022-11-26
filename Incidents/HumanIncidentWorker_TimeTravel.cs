using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_TimeTravel : HumanIncidentWorker {
    public const String Name = "TimeTravel";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();

        if (!(@params is HumanIncidentParams_TimeTravel)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_TimeTravel allParams = Tell.AssertNotNull((HumanIncidentParams_TimeTravel) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");


        Find.TickManager.DebugSetTicksGame(Mathf.RoundToInt(Find.TickManager.TicksGame + 2500 * allParams.HourChange.GetValue()));

        SendLetter(@params);

        return ir;
    }
}

public class HumanIncidentParams_TimeTravel : HumanIncidentParams {
    public Number HourChange = new Number(0);

    public HumanIncidentParams_TimeTravel() {
    }

    public HumanIncidentParams_TimeTravel(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, HourChange: [{HourChange}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Deep.Look(ref HourChange, "hourChange");
    }
}