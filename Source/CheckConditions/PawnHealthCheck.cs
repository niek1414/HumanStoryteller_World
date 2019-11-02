using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class PawnHealthCheck : CheckCondition {
        public const String Name = "PawnHealth";

        public static readonly Dictionary<string, HealthCondition> dict = new Dictionary<string, HealthCondition> {
            {"Alive", HealthCondition.Alive},
            {"Dead", HealthCondition.Dead},
            {"Healthy", HealthCondition.Healthy},
            {"InjuredOrDead", HealthCondition.InjuredOrDead},
            {"InjuredButAlive", HealthCondition.InjuredButAlive}
        };

        private String _pawnName;
        private HealthCondition _condition;

        public PawnHealthCheck() {
        }

        public PawnHealthCheck(string pawnName, HealthCondition condition) {
            _pawnName = Tell.AssertNotNull(pawnName, nameof(pawnName), GetType().Name);
            _condition = Tell.AssertNotNull(condition, nameof(condition), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Pawn pawn = PawnUtil.GetPawnByName(_pawnName);
            if (pawn.DestroyedOrNull()) {
                switch (_condition) {
                    case HealthCondition.Alive:
                        return false;
                    case HealthCondition.Dead:
                        return true;
                    case HealthCondition.Healthy:
                        return false;
                    case HealthCondition.InjuredOrDead:
                        return true;
                    case HealthCondition.InjuredButAlive:
                        return false;
                    default:
                        return false;
                }
            }

            switch (_condition) {
                case HealthCondition.Alive:
                    return !pawn.Dead;
                case HealthCondition.Dead:
                    return pawn.Dead;
                case HealthCondition.Healthy:
                    return !pawn.Dead && pawn.health.summaryHealth.SummaryHealthPercent > 0.8f;
                case HealthCondition.InjuredOrDead:
                    return pawn.Dead || pawn.health.summaryHealth.SummaryHealthPercent < 0.8f;
                case HealthCondition.InjuredButAlive:
                    return !pawn.Dead && pawn.health.summaryHealth.SummaryHealthPercent < 0.8f;
                default:
                    return false;
            }
        }

        public override string ToString() {
            return $"PawnName: [{_pawnName}], Condition: [{_condition}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _pawnName, "pawnName");
            Scribe_Values.Look(ref _condition, "condition");
        }
        
        public enum HealthCondition {
            Alive,
            Dead,
            Healthy,
            InjuredOrDead,
            InjuredButAlive
        }
    }
}