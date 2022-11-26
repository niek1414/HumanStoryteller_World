using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_MovieMode : HumanIncidentWorker {
    public const String Name = "MovieMode";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_MovieMode)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_MovieMode
            allParams = Tell.AssertNotNull((HumanIncidentParams_MovieMode) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        var sc = HumanStoryteller.StoryComponent;
        CameraJumper.TryHideWorld();
        sc.StoryStatus.MovieMode = allParams.Enable;
        if (allParams.Enable) {
            sc.StoryOverlay.AddItem(new BlackBars());
        }

        SendLetter(allParams);
        return ir;
    }
}

public class HumanIncidentParams_MovieMode : HumanIncidentParams {
    public bool Enable;

    public HumanIncidentParams_MovieMode() {
    }

    public HumanIncidentParams_MovieMode(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Enable: [{Enable}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Enable, "enable");
    }
}