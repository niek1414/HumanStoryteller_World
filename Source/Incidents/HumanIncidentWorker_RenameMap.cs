using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RenameMap : HumanIncidentWorker {
        public const String Name = "RenameMap";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();
            if (!(@params is HumanIncidentParams_RenameMap)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_RenameMap
                allParams = Tell.AssertNotNull((HumanIncidentParams_RenameMap) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");
            
            MapUtil.SaveMapByName(allParams.MapName, new MapContainer(((Map)allParams.GetTarget()).Parent));
            
            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_RenameMap : HumanIncidentParams {
        public string MapName = "";

        public HumanIncidentParams_RenameMap() {
        }

        public HumanIncidentParams_RenameMap(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, MapName: [{MapName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref MapName, "mapName");
        }
    }
}