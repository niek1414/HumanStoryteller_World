using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_BubbleMessage : HumanIncidentWorker {
    public const String Name = "BubbleMessage";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_BubbleMessage)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_BubbleMessage
            allParams = Tell.AssertNotNull((HumanIncidentParams_BubbleMessage) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        var pawn = PawnUtil.GetPawnByName(allParams.Name);
        if (pawn != null) {
            var message = new BubbleMessage(pawn, allParams.Message.Get() ?? "", allParams.BubbleType);
            HumanStoryteller.StoryComponent?.StoryOverlay.AddBubble(message);
        }

        SendLetter(allParams);
        return ir;
    }
}

public class HumanIncidentParams_BubbleMessage : HumanIncidentParams {
    public string Name = "";
    public RichText Message = new RichText();
    public BubbleMessage.BubbleType BubbleType = BubbleMessage.BubbleType.Normal;
    
    public HumanIncidentParams_BubbleMessage() {
    }

    public HumanIncidentParams_BubbleMessage(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Name: [{Name}], Message: [{Message}], Type: [{BubbleType}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Name, "name");
        Scribe_Deep.Look(ref Message, "message");
        Scribe_Values.Look(ref BubbleType, "bubbleType");
    }
}