using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Converters;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json.Linq;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Parser.Converter; 
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
        String type = GetString(obj, "type");
        if (type == null) {
            Parser.LogParseError("condition", type);
            return null;
        }

        switch (type) {
            case PawnHealthCheck.Name:
                return new PawnHealthCheck(GetString(obj, "pawnName"),
                    GetHealthCondition(GetString(obj, "healthCondition")));
            case PawnStateCheck.Name:
                return new PawnStateCheck(GetString(obj, "pawnName"),
                    GetPawnCondition(GetString(obj, "pawnCondition")));
            case MapCreatedCheck.Name:
                return new MapCreatedCheck(GetString(obj, "mapName"));
            case ColonistOnMapCheck.Name:
                return new ColonistOnMapCheck(GetString(obj, "mapName"),
                    GetString(obj, "pawnName"));
            case QuestCheck.Name:
                return new QuestCheck(GetQuestResponse(GetString(obj, "questState")));
            case TradeCheck.Name:
                return new TradeCheck(GetTradeResponse(GetString(obj, "tradeState")));
            case ColonistsOnMapCheck.Name:
                return new ColonistsOnMapCheck(GetString(obj, "mapName"),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case DialogCheck.Name:
                return new DialogCheck(GetDialogResponse(GetString(obj, "response")));
            case VariableCheck.Name:
                return new VariableCheck(GetString(obj, "name"),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case RandomCheck.Name:
                return new RandomCheck(GetNumber(obj["chance"]));
            case OnLocationCheck.Name:
                return CreateNewOnLocationCheck(obj);
            case DifficultyCheck.Name:
                return new DifficultyCheck(GetDifficulty(GetString(obj, "difficulty")));
            case TimeCheck.Name:
                return new TimeCheck(GetTimeList(obj, "hours"), GetTimeList(obj, "days"), GetTimeList(obj, "quadrums"),
                    GetTimeList(obj, "years"));
            case RelationCheck.Name:
                return new RelationCheck(
                    Find.FactionManager.AllFactions.First(f => f.def.defName == GetString(obj, "faction")),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case ItemMapCheck.Name:
                return new ItemMapCheck(
                    ThingDef.Named(GetString(obj, "item")),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case ItemColonyCheck.Name:
                return new ItemColonyCheck(
                    ThingDef.Named(GetString(obj, "item")),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case TemperatureCheck.Name:
                return new TemperatureCheck(
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case ColoniesCheck.Name:
                return new ColoniesCheck(
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case ColonistsCheck.Name:
                return new ColonistsCheck(
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case GroupCountCheck.Name:
                return new GroupCountCheck(
                    GetString(obj, "group"),
                    GetNumeralCompareResponse(GetString(obj, "compareType")),
                    GetNumber(obj["constant"])
                );
            case BiomeCheck.Name:
                return new BiomeCheck(obj["biomes"] != null ? obj["biomes"].Values<string>().ToList() : new List<string>());
            case AudioCheck.Name:
                return new AudioCheck();
            case TraveledCheck.Name:
                return new TraveledCheck();
            case CheatCheck.Name:
                return new CheatCheck();
            case CreatedStructureCheck.Name:
                return new CreatedStructureCheck();
            case CaravanLocationCheck.Name:
                return new CaravanLocationCheck();
            case QueueEventCheck.Name:
                return new QueueEventCheck();
            case ResearchCheck.Name:
                return new ResearchCheck(GetResearchProject(GetString(obj, "project")));
            case PlayerCanSeeCheck.Name:
                return new PlayerCanSeeCheck(GetSeeCondition(GetString(obj, "seeConditions")), GetLocation(obj["location"]));
            default:
                Parser.LogParseError("condition", type);
                return null;
        }
    }

    private CheckCondition CreateNewOnLocationCheck(JObject obj) {
        var filter = GetString(obj, "filterCategory");
        if (filter == null) {
            Parser.LogParseError("filterCategory", null);
            return null;
        }

        var atLeastAmount = GetNumber(obj["atLeastAmount"]);
        var location = GetLocation(obj["location"]);
        
        var category = Enum.Parse(typeof(FilterCategory), filter);
        switch (category) {
            case FilterCategory.Pawn:
                return new OnLocationCheck(location, GetGroupSelector(obj["pawnGroup"]), atLeastAmount);
            case FilterCategory.Category:
                return new OnLocationCheck(location, GetItemCategoryList(obj, "thingRequestGroups"), atLeastAmount);
            case FilterCategory.Item:
                return new OnLocationCheck(location, GetThingDef(GetString(obj, "item")), atLeastAmount);
            default:
                Parser.LogParseError("filterCategory", category.ToString());
                return null;
        }
    }

    private string GetString(JObject obj, string key) {
        return obj[key]?.Value<string>();
    }

    private Number GetNumber(JToken jToken) {
        if (jToken == null) {
            return new Number();
        }

        return (Number) new NumberJsonConverter().ReadJson(jToken, typeof(Number));
    }

    private PawnGroupSelector GetGroupSelector(JToken jToken) {
        if (jToken == null) {
            return null;
        }

        PawnGroupSelector selector = new PawnGroupSelector();
        JsonSerializer.Create(new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> {new NumberJsonConverter(), new DecimalJsonConverter(), new StringEnumConverter(), new PawnGroupConverter()}
        }).Populate(jToken.CreateReader(), selector);
        return selector;
    }

    private Location GetLocation(JToken jToken) {
        if (jToken == null) {
            return null;
        }

        Location location = new Location();
        JsonSerializer.Create(new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> {new NumberJsonConverter(), new DecimalJsonConverter()}
        }).Populate(jToken.CreateReader(), location);
        return location;
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

    private List<ThingRequestGroup> GetItemCategoryList(JObject obj, string categoryType) {
        var token = obj[categoryType];
        if (token == null) {
            return new List<ThingRequestGroup>();
        }

        IEnumerable<string> list = token.Values<string>();
        if (list == null) {
            Parser.LogParseError("item category - " + categoryType, null);
            return new List<ThingRequestGroup>();
        }

        var r = new List<ThingRequestGroup>();
        list.ToList().ForEach(item => { r.Add((ThingRequestGroup) Enum.Parse(typeof(ThingRequestGroup), item)); });

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

    private ThingDef GetThingDef(String type) {
        if (type == null) {
            Parser.LogParseError("thing def", type);
            return null;
        }

        var def = DefDatabase<ThingDef>.GetNamed(type, false);
        if (def != null) {
            return def;
        }

        Parser.LogParseError("thing def", type);
        return null;
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

    private PlayerCanSeeCheck.SeeConditions GetSeeCondition(String type) {
        if (type == null) {
            Parser.LogParseError("see condition", type);
            return PlayerCanSeeCheck.SeeConditions.FogAndViewport;
        }

        try {
            return (PlayerCanSeeCheck.SeeConditions) Enum.Parse(typeof(PlayerCanSeeCheck.SeeConditions), type);
        } catch (Exception e) {
            Parser.LogParseError("see condition, " + e.Message + "___" + e.StackTrace, type);
            return PlayerCanSeeCheck.SeeConditions.FogAndViewport;
        }
    }

    private PawnStateCheck.PawnCondition GetPawnCondition(String type) {
        if (type == null) {
            Parser.LogParseError("pawn condition", type);
            return PawnStateCheck.PawnCondition.Colonist;
        }

        try {
            return PawnStateCheck.dict[type];
        } catch (KeyNotFoundException) {
            Parser.LogParseError("pawn condition", type);
            return PawnStateCheck.PawnCondition.Colonist;
        }
    }

    private QuestResponse GetQuestResponse(String type) {
        if (type == null) {
            Parser.LogParseError("quest state", type);
            return QuestResponse.Pending;
        }

        try {
            return QuestCheck.dict[type];
        } catch (KeyNotFoundException) {
            Parser.LogParseError("quest state", type);
            return QuestResponse.Pending;
        }
    }

    private TradeResponse GetTradeResponse(String type) {
        if (type == null) {
            Parser.LogParseError("trade state", type);
            return TradeResponse.Pending;
        }

        try {
            return TradeCheck.dict[type];
        } catch (KeyNotFoundException) {
            Parser.LogParseError("trade state", type);
            return TradeResponse.Pending;
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

    private DataBankUtil.CompareType GetNumeralCompareResponse(String type) {
        if (type == null) {
            Parser.LogParseError("numeral compare", type);
            return DataBankUtil.CompareType.More;
        }

        try {
            return DataBankUtil.compareDict[type];
        } catch (KeyNotFoundException) {
            Parser.LogParseError("numeral compare", type);
            return DataBankUtil.CompareType.More;
        }
    }
}