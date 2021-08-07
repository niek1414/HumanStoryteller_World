using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShortEntry : HumanIncidentWorker {
        public const String Name = "ShortEntry";

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            Tell.Err("ShortEntry object should NEVER be executed!");
            throw new InvalidOperationException("HS_ ShortEntry object should NEVER be executed!");
        }
    }

    public class HumanIncidentParams_ShortEntry : HumanIncidentParams {
        public string Id = "";
        public string Name = "";
        public Number Commonality = new Number();
        
        public HumanIncidentParams_ShortEntry() {
        }

        public override string ToString() {
            return $"{base.ToString()}, Id: [{Id}], Name: [{Name}], Commonality: [{Commonality}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Id, "id");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Deep.Look(ref Commonality, "commonality");
        }
    }
}