using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_DisableStoryEvent : HumanIncidentWorker {
    public const String Name = "DisableStoryEvent";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_DisableStoryEvent)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_DisableStoryEvent
            allParams = Tell.AssertNotNull((HumanIncidentParams_DisableStoryEvent) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        var sc = HumanStoryteller.StoryComponent;
        sc.StoryStatus.DisableGameOverDialog = allParams.DisableGameOverDialog;
        sc.StoryStatus.DisableNameColonyDialog = allParams.DisableNameColonyDialog;

        SendLetter(allParams);
        return ir;
    }
}

public class HumanIncidentParams_DisableStoryEvent : HumanIncidentParams {
    public bool DisableGameOverDialog;
    public bool DisableNameColonyDialog;

    public HumanIncidentParams_DisableStoryEvent() {
    }

    public HumanIncidentParams_DisableStoryEvent(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, DisableGameOverDialog: [{DisableGameOverDialog}], DisableNameColonyDialog: [{DisableNameColonyDialog}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref DisableGameOverDialog, "disableGameOverDialog");
        Scribe_Values.Look(ref DisableNameColonyDialog, "disableNameColonyDialog");
    }
}