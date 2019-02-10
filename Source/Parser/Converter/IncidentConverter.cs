using System;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimWorld;

namespace HumanStoryteller.Parser.Converter {
   public class IncidentConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        private String defaultIncidentTarget = "OfPlayer";
        private HumanLetter defaultLetter = null;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(FiringHumanIncident);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            FiringHumanIncident incident;
            String type = jsonObject["type"].Value<string>();
            if (type == null) {
                Parser.LogParseError("incident", type);
                return LetterDefOf.NeutralEvent;
            }

            switch (type) {
                case HumanIncidentWorker_Nothing.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Nothing(),
                        new HumanIncidentParms(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_Alphabeavers.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_Alphabeavers(),
                        new HumanIncidentParams_Alphabeavers(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_FarmAnimalsWanderIn.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_FarmAnimalsWanderIn(),
                        new HumanIncidentParams_FarmAnimalsWanderIn(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_PsychicSoothe.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_PsychicSoothe(),
                        new HumanIncidentParams_PsychicSoothe(defaultIncidentTarget, defaultLetter));
                    break;
                case HumanIncidentWorker_RaidEnemy.Name:
                    incident = new FiringHumanIncident(new HumanIncidentWorker_RaidEnemy(),
                        new HumanIncidentParams_RaidEnemy(defaultIncidentTarget, defaultLetter));
                    break;
                default:
                    Parser.LogParseError("incident", type);
                    return new FiringHumanIncident(null);
            }

            serializer.Populate(jsonObject.CreateReader(), incident.Parms);
            return incident;
        }
    }
}