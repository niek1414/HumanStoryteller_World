using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.Parser.Converter {
    public class RichTextConverter : JsonConverter {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(RichText);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var s = "";
            try {
                switch (reader.TokenType) {
                    case JsonToken.String: {
                        return new RichText(JToken.ReadFrom(reader).Value<string>());
                    }
                }
            } catch (Exception) {
                Tell.Err("Error in parsing rich-text: expectedType: " + objectType + " JSONtype: " + reader.TokenType + " s: " + s);
                throw;
            }

            Tell.Err("Error in parsing rich-text: expectedType: " + objectType + " JSONtype: " + reader.TokenType + " s: " + s);
            return null;
        }

        public object ReadJson(JToken token, Type objectType) {
            var s = "";
            try {
                switch (token.Type) {
                    case JTokenType.String: {
                        return new RichText(token.Value<string>());
                    }
                }
            } catch (Exception) {
                Tell.Err("Error in parsing number: expectedType: " + objectType + " JSONtype: " + token.Type + " s: " + s);
                throw;
            }
            Tell.Err("Error in parsing number: expectedType: " + objectType + " JSONtype: " + token.Type + " s: " + s);
            return null;
        }
    }
}