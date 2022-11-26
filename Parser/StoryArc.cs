using System.Collections.Generic;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using Verse;

namespace HumanStoryteller.Parser; 
public class StoryArc {
    [JsonProperty(PropertyName = "id")]
    public long Id { get; set; }
    
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
    
    [JsonProperty(PropertyName = "creator")]
    public long Creator { get; set; }
    
    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }
    
    [JsonProperty(PropertyName = "gracePeriod")]
    public float GracePeriod { get; set; } // Only set on pack's
    
    [JsonProperty(PropertyName = "stories")]
    public List<Story> Stories { get; set; }

    public override string ToString() {
        return $"Id: [{Id}], Name: [{Name}], Creator: [{Creator}], Description: [{Description}], Stories: [{Stories.ToStringSafeEnumerable()}]";
    }
}