using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class QuestCheck : CheckCondition {
        public const String Name = "Quest";

        private QuestResponse _response;

        public static readonly Dictionary<string, QuestResponse> dict = new Dictionary<string, QuestResponse> {
            {"Pending", QuestResponse.Pending},
            {"Entered", QuestResponse.Entered},
            {"KilledAll", QuestResponse.KilledAll},
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
            if (!Find.WorldObjects.AnySiteAt(resultQuest.Parent.Tile)) {
                return _response == QuestResponse.Expired;
            }

            if (!resultQuest.Parent.HasMap) {
                return _response == QuestResponse.Pending;
            }

            if (!GenHostility.AnyHostileActiveThreatToPlayer(resultQuest.Parent.Map) && resultQuest.AnyEnemiesInitially) {
                return _response == QuestResponse.KilledAll;
            }

            return _response == QuestResponse.Entered;
        }

        public override string ToString() {
            return $"Response: {_response}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _response, "response");
        }
    }

    public enum QuestResponse {
        Pending,
        Entered,
        KilledAll,
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

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref Parent, "parent");
            Scribe_Values.Look(ref AnyEnemiesInitially, "anyEnemiesInitially");
        }
    }
}