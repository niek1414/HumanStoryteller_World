using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class TradeCheck : CheckCondition {
        public const String Name = "Trade";

        private TradeResponse _response;

        public static readonly Dictionary<string, TradeResponse> dict = new Dictionary<string, TradeResponse> {
            {"Pending", TradeResponse.Pending},
            {"Expired", TradeResponse.Expired}
        };

        public TradeCheck() {
        }

        public TradeCheck(TradeResponse response) {
            _response = Tell.AssertNotNull(response, nameof(response), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            if (result == null) {
                Tell.Err($"Tried to check {GetType()} but result type was null." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            if (!(result is IncidentResult_Trade)) {
                Tell.Err($"Tried to check {GetType()} but result type was {result.GetType()}." +
                         " Likely the storycreator added a incomparable condition to an event.");
                return false;
            }

            IncidentResult_Trade resultTrade = (IncidentResult_Trade) result;
            if (!Find.WorldObjects.AnySiteAt(resultTrade.Parent.Tile)) {
                return _response == TradeResponse.Expired;
            }

            return _response == TradeResponse.Pending;
        }

        public override string ToString() {
            return $"Response: [{_response}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _response, "response");
        }
    }

    public enum TradeResponse {
        Pending,
        Expired
    }

    public class IncidentResult_Trade : IncidentResult {
        public MapParent Parent;

        public IncidentResult_Trade() {
        }

        public IncidentResult_Trade(MapParent parent) {
            Parent = parent;
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_References.Look(ref Parent, "parent");
        }
    }
}