using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CreateStructure : HumanIncidentWorker {
        public const String Name = "CreateStructure";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_CreateStructure)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_CreateStructure allParams =
                Tell.AssertNotNull((HumanIncidentParams_CreateStructure) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            if (allParams.Structure != "") {
                IntVec3 cell = allParams.Location.GetSingleCell(map, false);
                if (!AreaUtil.StringToAreaObjects(allParams.Structure, map, cell.IsValid ? cell : IntVec3.Zero)) {
                    Tell.Warn("Unable to translate string into spawnable objects. For more info ^");
                }
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_CreateStructure : HumanIncidentParms {
        public string Structure = "";
        public Location Location = new Location();

        public HumanIncidentParams_CreateStructure() {
        }

        public HumanIncidentParams_CreateStructure(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Structure: {Structure}, Location: {Location}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Structure, "structure");
            Scribe_Deep.Look(ref Location, "location");
        }
    }
}