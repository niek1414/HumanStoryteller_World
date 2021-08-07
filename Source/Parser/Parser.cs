using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using HumanStoryteller.Parser.Converter;
using HumanStoryteller.Util.Logging;
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

        public static Model.StoryPart.StoryArc StoryParser(String json) {
            var storyArc = JsonConvert.DeserializeObject<StoryArc>(json, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> {
                    new NumberJsonConverter(),
                    new RichTextConverter(),
                    new DecimalJsonConverter(),
                    new StringEnumConverter(),
                    new PawnGroupConverter()
                }
            });

            var stories = new List<StoryGraph>();
            foreach (var story in storyArc.Stories) {
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
                    storyNode.Conditions = parseNode.Conditions;
                    storyNode.Modifications = parseNode.Modifications;
                    if (parseNode.Left != null) {
                        try {
                            storyNode.LeftChild =
                                new Model.StoryPart.Connection(parseNode.Left.Offset, nodesDictionary[parseNode.Left.Uuid].RealNode);
                        } catch (KeyNotFoundException) {
                            Tell.Err("While parsing, a unknown node was mentioned. UUID:" + parseNode.Left);
                            Tell.Warn(json);
                        }
                    }

                    if (parseNode.Right != null) {
                        try {
                            storyNode.RightChild =
                                new Model.StoryPart.Connection(parseNode.Right.Offset, nodesDictionary[parseNode.Right.Uuid].RealNode);
                        } catch (KeyNotFoundException) {
                            Tell.Err("While parsing, a unknown node was mentioned. UUID:" + parseNode.Right);
                            Tell.Warn(json);
                        }
                    }
                }

                if (root == null) {
                    Tell.Warn("Unable to read the root of story " + story.Id);
                    continue;
                }

                switch (root.StoryEvent.Incident.Params) {
                    case HumanIncidentParams_LongEntry _:
                        stories.Add(new LongStoryGraph(story.Id, root));
                        break;
                    case HumanIncidentParams_ShortEntry _:
                        stories.Add(new ShortStoryGraph(story.Id, root));
                        break;
                    default:
                        Tell.Warn("Unexpected root type (" + root.StoryEvent.Incident.Params.GetType() + ") of story: " + story.Id);
                        break;
                }
            }

            return new Model.StoryPart.StoryArc(storyArc.Id, storyArc.Name, storyArc.Description, storyArc.Creator, stories);
        }


        public static void LogParseError(String type, String value) {
            Tell.Err("Unknown " + type + " type while parsing (type = null), consider updating the mod. Value was " +
                     (value ?? "null"));
        }
    }
}