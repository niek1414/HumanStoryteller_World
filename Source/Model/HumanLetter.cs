using HumanStoryteller.Parser.Converter;
using Newtonsoft.Json;
using Verse;

namespace HumanStoryteller.Model {
    public class HumanLetter : IExposable {
        public string Title;
        public string Message;

        [JsonConverter(typeof(LetterTypeConverter))]
        public LetterDef Type;
        public bool Show;
        public bool Shake;
        public bool Force;

        public HumanLetter() {
        }

        public HumanLetter(string title, string message, LetterDef type, bool show, bool shake, bool force) {
            Title = title;
            Message = message;
            Type = type;
            Show = show;
            Shake = shake;
            Force = force;
        }
        
        public void ExposeData() {
            Scribe_Values.Look(ref Title, "title");
            Scribe_Values.Look(ref Message, "message");
            Scribe_Defs.Look(ref Type, "type");
            Scribe_Values.Look(ref Show, "show");
            Scribe_Values.Look(ref Shake, "shake");
            Scribe_Values.Look(ref Force, "force");
        }
    }
}