using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_CoupleDecouple : HumanIncidentWorker {
    public const String Name = "CoupleDecouple";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_CoupleDecouple)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_CoupleDecouple allParams = Tell.AssertNotNull((HumanIncidentParams_CoupleDecouple) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");
        var mapContainer = MapUtil.GetMapContainerByTile(allParams.Target.GetTileFromTarget());
        if (mapContainer == null) {
            mapContainer = new MapContainer(((Map) allParams.GetTarget()).Parent);
            MapUtil.SaveMapByName("Generated_" + Guid.NewGuid(), mapContainer);
        } else {
            mapContainer.FakeDisconnect();
        }
        if (allParams.Couple) {
            mapContainer.TryCouple();
        } else if (!allParams.Permanent) {
            mapContainer.Decouple();
        } else {
            mapContainer.Remove();
        }
        
        SendLetter(allParams);

        return ir;
    }
}

public class HumanIncidentParams_CoupleDecouple : HumanIncidentParams {
    public bool Couple;
    public bool Permanent;

    public HumanIncidentParams_CoupleDecouple() {
    }

    public HumanIncidentParams_CoupleDecouple(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return $"{base.ToString()}, Couple: [{Couple}], Permanent: [{Permanent}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref Couple, "couple");
        Scribe_Values.Look(ref Permanent, "permanent");
    }
}