using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Parser.Converter {
    public class ConditionConverter : JsonConverter {
        public override bool CanWrite => false;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(List<CheckCondition>);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new InvalidOperationException("Use default serialization.");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            List<CheckCondition> conditions = new List<CheckCondition>();
            if (reader.TokenType == JsonToken.StartObject) {
                conditions.Add(GetCondition(JObject.Load(reader)));
            } else {
                JArray array = JArray.Load(reader);
                foreach (var jToken in array) {
                    var item = (JObject) jToken;
                    conditions.Add(GetCondition(item));
                }
            }

            return conditions;
        }

        private CheckCondition GetCondition(JObject obj) {
            String type = obj["type"].Value<string>();
            if (type == null) {
                Parser.LogParseError("condition", type);
                return null;
            }

            switch (type) {
                case PawnHealthCheck.Name:
                    return new PawnHealthCheck(obj["pawnName"].Value<string>(),
                        GetHealthCondition(obj["healthCondition"].Value<string>()));
                case DialogCheck.Name:
                    return new DialogCheck(GetDialogResponse(obj["response"].Value<string>()));
                case VariableCheck.Name:
                    return new VariableCheck(obj["name"].Value<string>(),
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case RandomCheck.Name:
                    return new RandomCheck(float.Parse(obj["chance"].Value<string>()));
                case DifficultyCheck.Name:
                    return new DifficultyCheck(GetDifficulty(obj["difficulty"].Value<string>()));
                case TimeCheck.Name:
                    return new TimeCheck(GetTimeList(obj, "hours"), GetTimeList(obj, "days"), GetTimeList(obj, "quadrums"),
                        GetTimeList(obj, "years"));
                case RelationCheck.Name:
                    return new RelationCheck(
                        Find.FactionManager.AllFactions.First(f => f.def.defName == obj["faction"].Value<string>()),
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case ItemMapCheck.Name:
                    return new ItemMapCheck(
                        ThingDef.Named(obj["item"].Value<string>()),
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case ItemColonyCheck.Name:
                    return new ItemColonyCheck(
                        ThingDef.Named(obj["item"].Value<string>()),
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case TemperatureCheck.Name:
                    return new TemperatureCheck(
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case ColonistsCheck.Name:
                    return new ColonistsCheck(
                        GetNumeralCompareResponse(obj["compareType"].Value<string>()),
                        float.Parse(obj["constant"].Value<string>(), CultureInfo.InvariantCulture.NumberFormat)
                    );
                case BiomeCheck.Name:
                    return new BiomeCheck(obj["biomes"]!= null?obj["biomes"].Values<string>().ToList():new List<string>());
                case AudioCheck.Name:
                    return new AudioCheck();
                case CheatCheck.Name:
                    return new CheatCheck();
                case ResearchCheck.Name:
                    return new ResearchCheck(GetResearchProject(obj["project"].Value<string>()));
                default:
                    Parser.LogParseError("condition", type);
                    return null;
            }
        }

        private List<int> GetTimeList(JObject obj, string timeType) {
            var token = obj[timeType];
            if (token == null) {
                return new List<int>();
            }
            IEnumerable<string> list = token.Values<string>();
            if (list == null) {
                Parser.LogParseError("time - " + timeType, null);
                return new List<int>();
            }

            List<int> r = new List<int>();
            list.ToList().ForEach(item => { r.Add(int.Parse(item, CultureInfo.InvariantCulture.NumberFormat)); });

            return r;
        }

        private ResearchProjectDef GetResearchProject(String type) {
            if (type == null) {
                Parser.LogParseError("research project", type);
                return ResearchProjectDefOf.CarpetMaking;
            }

            var def = DefDatabase<ResearchProjectDef>.GetNamed(type, false);
            if (def != null) {
                return def;
            }

            Parser.LogParseError("research project", type);
            return ResearchProjectDefOf.CarpetMaking;
        }

        private DifficultyDef GetDifficulty(String type) {
            if (type == null) {
                Parser.LogParseError("difficulty", type);
                return DifficultyDefOf.Rough;
            }

            var def = DefDatabase<DifficultyDef>.GetNamed(type, false);
            if (def != null) {
                return def;
            }

            Parser.LogParseError("difficulty", type);
            return DifficultyDefOf.Rough;
        }

        private PawnHealthCheck.HealthCondition GetHealthCondition(String type) {
            if (type == null) {
                Parser.LogParseError("health condition", type);
                return PawnHealthCheck.HealthCondition.Alive;
            }

            try {
                return PawnHealthCheck.dict[type];
            } catch (KeyNotFoundException) {
                Parser.LogParseError("health condition", type);
                return PawnHealthCheck.HealthCondition.Alive;
            }
        }

        private DialogResponse GetDialogResponse(String type) {
            if (type == null) {
                Parser.LogParseError("dialog response", type);
                return DialogResponse.Accepted;
            }

            try {
                return DialogCheck.dict[type];
            } catch (KeyNotFoundException) {
                Parser.LogParseError("dialog response", type);
                return DialogResponse.Accepted;
            }
        }

        private DataBank.CompareType GetNumeralCompareResponse(String type) {
            if (type == null) {
                Parser.LogParseError("numeral compare", type);
                return DataBank.CompareType.More;
            }

            try {
                return DataBank.compareDict[type];
            } catch (KeyNotFoundException) {
                Parser.LogParseError("numeral compare", type);
                return DataBank.CompareType.More;
            }
        }
    }
}