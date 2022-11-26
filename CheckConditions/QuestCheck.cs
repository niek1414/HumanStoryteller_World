using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.CheckConditions; 
public class QuestCheck : CheckCondition {
    public const String Name = "Quest";

    private QuestResponse _response;

    public static readonly Dictionary<string, QuestResponse> dict = new Dictionary<string, QuestResponse> {
        {"Pending", QuestResponse.Pending},
        {"Entered", QuestResponse.Entered},
        {"Expired", QuestResponse.Expired}
    };

    public QuestCheck() {
    }

    public QuestCheck(QuestResponse response) {
        _response = Tell.AssertNotNull(response, nameof(response), GetType().Name);
    }

    public override bool Check(IncidentResult result, int checkPosition) {
        if (result == null) {
            Tell.Err($"Tried to check {GetType()} but result type was null." +
                     " Likely the storycreator added a incomparable condition to an event.");
            return false;
        }

        if (!(result is IncidentResult_Quest)) {
            Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                     " Likely the storycreator added a incomparable condition to an event.");
            return false;
        }

        IncidentResult_Quest resultQuest = (IncidentResult_Quest) result;
        if (resultQuest.Parent == null || !Find.WorldObjects.AnySiteAt(resultQuest.Parent.Tile)) {
            return _response == QuestResponse.Expired;
        }

        if (!resultQuest.Parent.HasMap) {
            return _response == QuestResponse.Pending;
        }
        
        return _response == QuestResponse.Entered;
    }

    public override string ToString() {
        return $"Response: [{_response}]";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref _response, "response");
    }
}

public enum QuestResponse {
    Pending,
    Entered,
    Expired
}

public class IncidentResult_Quest : IncidentResult {
    public MapParent Parent;
    public bool AnyEnemiesInitially;

    public IncidentResult_Quest() {
    }

    public IncidentResult_Quest(MapParent parent, bool anyEnemiesInitially) {
        Parent = parent;
        AnyEnemiesInitially = anyEnemiesInitially;
    }

    public override string ToString() {
        return $"{base.ToString()}, Parent tile: {Parent.Tile}, AnyEnemiesInitially: {AnyEnemiesInitially}";
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref Parent, "parent");
        Scribe_Values.Look(ref AnyEnemiesInitially, "anyEnemiesInitially");
    }
}