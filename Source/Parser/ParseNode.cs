using System.Collections.Generic;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using Newtonsoft.Json;

namespace HumanStoryteller.Parser {
    public class ParseNode {
        public string Uuid { get; set; }
        public string Name { get; set; }
        public Connection Left { get; set; }
        public Connection Right { get; set; }

        [JsonConverter(typeof(ConditionConverter))]
        public List<CheckCondition> Conditions { get; set; }

        [JsonIgnore]
        public StoryNode RealNode { get; set; }

        [JsonConverter(typeof(IncidentConverter))]
        public FiringHumanIncident Incident { get; set; }

        public override string ToString() {
            return $"Uuid: {Uuid}, Left: {Left}, Right: {Right}, Condition: {Conditions}, RealNode: {RealNode}, Incident: {Incident}";
        }
    }
}