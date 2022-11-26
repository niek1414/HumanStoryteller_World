using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using Verse;
using Pointer = HumanStoryteller.Util.Overlay.Pointer;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_PointTo : HumanIncidentWorker {
    public const String Name = "PointTo";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_PointTo)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_PointTo
            allParams = Tell.AssertNotNull((HumanIncidentParams_PointTo) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();
        
        var sc = HumanStoryteller.StoryComponent;
        if (!allParams.RemoveAll) {
            sc.StoryOverlay.AddItem(new Pointer(map, allParams.Location.GetSingleCell(map)));
        } else {
            sc.StoryOverlay.NotifyEnd<Pointer>();
        }

        SendLetter(allParams);
        return ir;
    }
}

public class HumanIncidentParams_PointTo : HumanIncidentParams {
    public bool RemoveAll;
    public Location Location = new Location();

    public HumanIncidentParams_PointTo() {
    }

    public HumanIncidentParams_PointTo(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, RemoveAll: [{RemoveAll}], Location: [{Location}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref RemoveAll, "removeAll");
        Scribe_Deep.Look(ref Location, "location");
    }
}