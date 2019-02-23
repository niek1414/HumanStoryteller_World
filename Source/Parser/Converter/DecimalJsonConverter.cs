using System;
using System.Globalization;
using HumanStoryteller.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HumanStoryteller.Parser.Converter {
    public class DecimalJsonConverter : JsonConverter {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(float) || objectType == typeof(long);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            string s = "";
            try {
                if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer) {
                    if (objectType == typeof(float)) {
                        return JToken.ReadFrom(reader).Value<float>();
                    }

                    if (objectType == typeof(long)) {
                        return JToken.ReadFrom(reader).Value<long>();
                    }
                }

                if (reader.TokenType == JsonToken.String) {
                    s = JToken.ReadFrom(reader).Value<string>();
                    if (s.Length > 0) {
                        if (objectType == typeof(float)) {
                            return float.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                        }

                        if (objectType == typeof(long)) {
                            return long.Parse(s, CultureInfo.InvariantCulture.NumberFormat);
                        }
                    }
                }

                if (objectType == typeof(float)) {
                    return -1f;
                }
            } catch (Exception) {
                Tell.Err("Error in parsing number: expectedType: " + objectType + " JSONtype: " + reader.TokenType + " s: " + s);
                throw;
            }

            return -1L;
        }
    }
}