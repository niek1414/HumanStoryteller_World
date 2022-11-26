using System;
using System.Globalization;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using HumanStoryteller.Util.Logging;

namespace HumanStoryteller.Parser.Converter; 
public class NumberJsonConverter : JsonConverter {
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) {
        return objectType == typeof(Number);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        throw new InvalidOperationException("Use default serialization.");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        var s = "";
        try {
            switch (reader.TokenType) {
                case JsonToken.Float:
                case JsonToken.Integer: {
                    return new Number(JToken.ReadFrom(reader).Value<float>());
                }
                case JsonToken.String: {
                    s = JToken.ReadFrom(reader).Value<string>();
                    if (s.Length > 0) {
                        if (s.StartsWith("v_")) {
                            return new Number(s.Substring(2));
                        }
                        return new Number(float.Parse(s, CultureInfo.InvariantCulture.NumberFormat));
                    }

                    break;
                }
            }

            return new Number();
        } catch (Exception) {
            Tell.Err("Error in parsing number: expectedType: " + objectType + " JSONtype: " + reader.TokenType + " s: " + s);
            throw;
        }
    }
    
    public object ReadJson(JToken token, Type objectType) {
        var s = "";
        try {
            switch (token.Type) {
                case JTokenType.Float:
                case JTokenType.Integer: {
                    return new Number(token.Value<float>());
                }
                case JTokenType.String: {
                    s = token.Value<string>();
                    if (s.Length > 0) {
                        if (s.StartsWith("v_")) {
                            return new Number(s.Substring(2));
                        }
                        return new Number(float.Parse(s, CultureInfo.InvariantCulture.NumberFormat));
                    }

                    break;
                }
            }

            return new Number();
        } catch (Exception) {
            Tell.Err("Error in parsing number: expectedType: " + objectType + " JSONtype: " + token.Type + " s: " + s);
            throw;
        }
    }
}