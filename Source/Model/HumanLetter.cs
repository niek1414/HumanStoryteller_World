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

        public HumanLetter() {
        }

        public HumanLetter(string title, string message, LetterDef type, bool show) {
            Title = title;
            Message = message;
            Type = type;
            Show = show;
        }
        
        public void ExposeData() {
            Scribe_Values.Look(ref Title, "title");
            Scribe_Values.Look(ref Message, "message");
            Scribe_Defs.Look(ref Type, "type");
            Scribe_Values.Look(ref Show, "show");
        }
    }
}