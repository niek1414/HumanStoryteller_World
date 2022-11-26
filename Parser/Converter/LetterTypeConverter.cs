using System;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Parser.Converter; 
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