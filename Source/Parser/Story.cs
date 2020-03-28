using System.Collections.Generic;
using Newtonsoft.Json;

namespace HumanStoryteller.Parser {
    public class Story {
        public string Name { get; set; }
        public string Description { get; set; }
        [JsonProperty(PropertyName = "story")]
        public List<ParseNode> StoryGraph { get; set; }

        public override string ToString() {
            return $"Name: [{Name}], Description: [{Description}], StoryGraph: [{StoryGraph}]";
        }
    }
}