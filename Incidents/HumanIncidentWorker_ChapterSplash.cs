using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_ChapterSplash : HumanIncidentWorker {
    public const String Name = "ChapterSplash";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_ChapterSplash)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_ChapterSplash
            allParams = Tell.AssertNotNull((HumanIncidentParams_ChapterSplash) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");
        if (allParams.Title != "" || allParams.Description != "") {
            HumanStoryteller.StoryComponent?.StoryOverlay.AddItem(new ChapterBar(allParams.Title, allParams.Description));
        }

        SendLetter(allParams);
        return ir;
    }
}

public class HumanIncidentParams_ChapterSplash : HumanIncidentParams {
    public string Title = "";
    public string Description = "";

    public HumanIncidentParams_ChapterSplash() {
    }

    public HumanIncidentParams_ChapterSplash(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Title: [{Title}], Description: [{Description}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Title, "title");
        Scribe_Values.Look(ref Description, "description");
    }
}