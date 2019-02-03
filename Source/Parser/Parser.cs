using System;
using System.Collections.Generic;
using System.Globalization;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Parser {
    public static class Parser {
        public static StorySummary[] SummaryParser(String json) {
            JArray stories = JArray.Parse(json);
            StorySummary[] result = new StorySummary[stories.Count];

            for (var i = 0; i < stories.Count; i++) {
                JToken story = stories[i];
                result[i] = JsonConvert.DeserializeObject<StorySummary>(story.ToString(Formatting.None), new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore,
                    Converters = new List<JsonConverter> {new DecimalJsonConverter()}
                });
            }

            return result;
        }

        public static Model.Story StoryParser(String json) {
            JObject mainStory = JObject.Parse(json);
            var story = JsonConvert.DeserializeObject<Story>(mainStory["storyline"].ToString(Formatting.None), new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> {new DecimalJsonConverter()}
            });

            var nodesDictionary = new Dictionary<string, ParseNode>();
            StoryNode root = null;
            foreach (var parseNode in story.StoryGraph) {
                var storyEvent = new StoryEvent(parseNode.Uuid, parseNode.Name, parseNode.Incident);
                StoryNode storyNode = new StoryNode(storyEvent);
                parseNode.RealNode = storyNode;
                if (parseNode.Uuid.EqualsIgnoreCase("root")) {
                    root = storyNode;
                } else if (parseNode.Uuid.StartsWith("D__")) {
                    parseNode.Uuid = parseNode.Uuid.Substring(3);
                    storyEvent.Uuid = parseNode.Uuid;
                    storyNode.Divider = true;
                }

                nodesDictionary.Add(parseNode.Uuid, parseNode);
            }

            foreach (ParseNode parseNode in story.StoryGraph) {
                var storyNode = parseNode.RealNode;
                if (parseNode.Left != null) {
                    try {
                        storyNode.LeftChild = new Model.Connection(parseNode.Left.Offset, nodesDictionary[parseNode.Left.Uuid].RealNode);
                    } catch (KeyNotFoundException e) {
                        Tell.Err("While parsing, a unknown node was mentioned. UUID:" + parseNode.Left);
                        Tell.Log(json);
                    }
                }

                if (parseNode.Right != null) {
                    try {
                        storyNode.RightChild = new Model.Connection(parseNode.Right.Offset, nodesDictionary[parseNode.Right.Uuid].RealNode);
                        storyNode.Conditions = parseNode.Conditions;
                    } catch (KeyNotFoundException e) {
                        Tell.Err("While parsing, a unknown node was mentioned. UUID:" + parseNode.Right);
                        Tell.Log(json);
                    }
                }
            }

            return new Model.Story(mainStory["id"].Value<long>(), story.Name, story.Description, mainStory["creator"].Value<long>(),
                new StoryGraph(root));
        }


        public static void LogParseError(String type, String value) {
            Tell.Err("Unknown " + type + " type while parsing (type = null), consider updating the mod. Value was " +
                      (value == null ? "null" : value));
        }
    }

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

    public class LetterTypeConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(LetterDef);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            String type = JToken.ReadFrom(reader).Value<string>();
            if (type == null) {
                //Parser.LogParseError("letter", type);
                return LetterDefOf.NeutralEvent;
            }

            switch (type) {
                case "Default":
                    return null;
                case "ThreatBig":
                    return LetterDefOf.ThreatBig;
                case "ThreatSmall":
                    return LetterDefOf.ThreatSmall;
                case "NegativeEvent":
                    return LetterDefOf.NegativeEvent;
                case "NeutralEvent":
                    return LetterDefOf.NeutralEvent;
                case "PositiveEvent":
                    return LetterDefOf.PositiveEvent;
                case "Death":
                    return LetterDefOf.Death;
                default:
                    Parser.LogParseError("letter", type);
                    return LetterDefOf.NeutralEvent;
            }
        }
    }

    public class ConditionConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(List<CheckCondition>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            List<CheckCondition> conditions = new List<CheckCondition>();
            if (reader.TokenType == JsonToken.StartObject) {
                conditions.Add(GetCondition(JObject.Load(reader)));
            } else {
                JArray array = JArray.Load(reader);
                foreach (var jToken in array) {
                    var item = (JObject) jToken;
                    conditions.Add(GetCondition(item));
                }
            }

            return conditions;
        }

        private CheckCondition GetCondition(JObject obj) {
            String type = obj["type"].Value<string>();
            if (type == null) {
                Parser.LogParseError("condition", type);
                return null;
            }

            switch (type) {
                case PawnHealthCheck.Name:
                    return new PawnHealthCheck(obj["pawnName"].Value<string>(),
                        GetHealthCondition(obj["healthCondition"].Value<string>()));
                default:
                    Parser.LogParseError("condition", type);
                    return null;
            }
        }

        private HealthCondition GetHealthCondition(String type) {
            if (type == null) {
                Parser.LogParseError("health condition", type);
                return HealthCondition.Alive;
            }

            try {
                return PawnHealthCheck.dict[type];
            } catch (KeyNotFoundException e) {
                Parser.LogParseError("health condition", type);
                return HealthCondition.Alive;
            }
        }
    }

    class DecimalJsonConverter : JsonConverter {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(float);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer) {
                return JToken.ReadFrom(reader).Value<float>();
            }

            if (reader.TokenType == JsonToken.String) {
                return float.Parse(JToken.ReadFrom(reader).Value<string>(), CultureInfo.InvariantCulture.NumberFormat);
            }

            return -1;
        }
    }
}