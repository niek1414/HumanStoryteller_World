using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_DestroyPosition : HumanIncidentWorker {
        public const String Name = "DestroyPosition";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_DestroyPosition)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_DestroyPosition allParams =
                Tell.AssertNotNull((HumanIncidentParams_DestroyPosition) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            allParams.Location.GetZone(map).Cells.ForEach(cell => {
                cell.Pos.GetThingList(map).ForEach(thing => {
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
                    }

                    thing.Destroy();
                });
            });

            SendLetter(parms);
            return ir;
        }
    }

    public class HumanIncidentParams_DestroyPosition : HumanIncidentParms {
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
                $"{base.ToString()}, DestroyItems: {DestroyItems}, DestroyPawns: {DestroyPawns}, DestroyStructures: {DestroyStructures}, DestroyPlants: {DestroyPlants}, Location: {Location}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref DestroyItems, "destroyItems");
            Scribe_Values.Look(ref DestroyPawns, "destroyPawns");
            Scribe_Values.Look(ref DestroyStructures, "destroyStructures");
            Scribe_Values.Look(ref DestroyPlants, "destroyPlants");
            Scribe_Deep.Look(ref Location, "location");
        }
    }
}