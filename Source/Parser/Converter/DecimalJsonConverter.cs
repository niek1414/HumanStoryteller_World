using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HumanStoryteller.Parser.Converter {
    public class DecimalJsonConverter : JsonConverter {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(float);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer) {
                return JToken.ReadFrom(reader).Value<float>();
            }

            if (reader.TokenType == JsonToken.String) {
                return float.Parse(JToken.ReadFrom(reader).Value<string>(), CultureInfo.InvariantCulture.NumberFormat);
            }

            return -1;
        }
    }
}