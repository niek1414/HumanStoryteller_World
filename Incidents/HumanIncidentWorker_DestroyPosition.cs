using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents; 
class HumanIncidentWorker_DestroyPosition : HumanIncidentWorker {
    public const String Name = "DestroyPosition";

    protected override IncidentResult Execute(HumanIncidentParams @params) {
        IncidentResult ir = new IncidentResult();
        if (!(@params is HumanIncidentParams_DestroyPosition)) {
            Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
            return ir;
        }

        HumanIncidentParams_DestroyPosition allParams =
            Tell.AssertNotNull((HumanIncidentParams_DestroyPosition) @params, nameof(@params), GetType().Name);
        Tell.Log($"Executing event {Name} with:{allParams}");

        Map map = (Map) allParams.GetTarget();

        var list = allParams.Location.GetZone(map).Cells;
        list.ForEach(cell => {
            if (allParams.DestroyRoof) {
                map.roofGrid.SetRoof(cell.Pos, null);
            }
            cell.Pos.GetThingList(map).ListFullCopy().ForEach(thing => {
                if (!thing.def.destroyable) return;
                switch (thing.def.category) {
                    case ThingCategory.None:
                        return;
                    case ThingCategory.Pawn:
                        if (!allParams.DestroyPawns) return;
                        break;
                    case ThingCategory.Item:
                        if (!allParams.DestroyItems) return;
                        break;
                    case ThingCategory.Building:
                        if (!allParams.DestroyStructures) return;
                        break;
                    case ThingCategory.Plant:
                        if (!allParams.DestroyPlants) return;
                        break;
                    case ThingCategory.Projectile:
                        return;
                    case ThingCategory.Filth:
                        break;
                    case ThingCategory.Gas:
                        return;
                    case ThingCategory.Attachment:
                        return;
                    case ThingCategory.Mote:
                        return;
                    case ThingCategory.Ethereal:
                        return;
                    default:
                        return;
                }

                if (thing.def.destroyable) {
                    thing.Destroy();
                }
            });
        });

        SendLetter(@params);
        return ir;
    }
}

public class HumanIncidentParams_DestroyPosition : HumanIncidentParams {
    public bool DestroyRoof;
    public bool DestroyItems;
    public bool DestroyPawns;
    public bool DestroyStructures;
    public bool DestroyPlants;
    public Location Location = new Location();

    public HumanIncidentParams_DestroyPosition() {
    }

    public HumanIncidentParams_DestroyPosition(Target target, HumanLetter letter) : base(target, letter) {
    }

    public override string ToString() {
        return
            $"{base.ToString()}, DestroyItems: [{DestroyRoof}],  DestroyItems: [{DestroyItems}], DestroyPawns: [{DestroyPawns}], DestroyStructures: [{DestroyStructures}], DestroyPlants: [{DestroyPlants}], Location: [{Location}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref DestroyRoof, "DestroyRoof");
        Scribe_Values.Look(ref DestroyItems, "destroyItems");
        Scribe_Values.Look(ref DestroyPawns, "destroyPawns");
        Scribe_Values.Look(ref DestroyStructures, "destroyStructures");
        Scribe_Values.Look(ref DestroyPlants, "destroyPlants");
        Scribe_Deep.Look(ref Location, "location");
    }
}