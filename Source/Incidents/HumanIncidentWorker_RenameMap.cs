using System;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RenameMap : HumanIncidentWorker {
        public const String Name = "RenameMap";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_RenameMap)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RenameMap
                allParams = Tell.AssertNotNull((HumanIncidentParams_RenameMap) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");
            
            MapUtil.SaveMapByName(allParams.MapName, new MapContainer(((Map)allParams.GetTarget()).Parent));
            
            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_RenameMap : HumanIncidentParms {
        public string MapName = "";

        public HumanIncidentParams_RenameMap() {
        }

        public HumanIncidentParams_RenameMap(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, MapName: {MapName}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref MapName, "mapName");
        }
    }
}