using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Parser.Converter;
using HumanStoryteller.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
}