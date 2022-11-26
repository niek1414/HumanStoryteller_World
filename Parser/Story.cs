using System.Collections.Generic;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using Verse;

namespace HumanStoryteller.Parser; 
public class Story {
    [JsonProperty(PropertyName = "uuid")]
    public string Id { get; set; }
    
    [JsonProperty(PropertyName = "graph")]
    public List<ParseNode> StoryGraph { get; set; }

    public override string ToString() {
        return $"Id: [{Id}], StoryGraph: [{StoryGraph.ToStringSafeEnumerable()}]";
    }
}