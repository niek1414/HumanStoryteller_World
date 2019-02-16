using HumanStoryteller.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Verse;

namespace HumanStoryteller.Model {
    public class VariableModifications : IExposable {
        public string Name;
        
        [JsonConverter(typeof(StringEnumConverter))]
        public ModificationType Modification;
        public float Constant;

        public VariableModifications() {
        }

        public VariableModifications(string name, ModificationType modification, float constant) {
            Name = name;
            Modification = modification;
            Constant = constant;
        }

        public override string ToString() {
            return $"Name: {Name}, Modification: {Modification}, Constant: {Constant}";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref Modification, "modification");
            Scribe_Values.Look(ref Constant, "constant");
        }
    }

    public enum ModificationType {
        Add,
        Subtract,
        Divide,
        Multiply,
        Equal
    }
}