using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class PawnStateCheck : CheckCondition {
        public const String Name = "PawnState";

        public static readonly Dictionary<string, PawnCondition> dict = new Dictionary<string, PawnCondition> {
            {"Colonist", PawnCondition.Colonist},
            {"Prisoner", PawnCondition.Prisoner}
        };

        private String _pawnName;
        private PawnCondition _condition;

        public PawnStateCheck() {
        }

        public PawnStateCheck(string pawnName, PawnCondition condition) {
            _pawnName = Tell.AssertNotNull(pawnName, nameof(pawnName), GetType().Name);
            _condition = Tell.AssertNotNull(condition, nameof(condition), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            Pawn pawn = PawnUtil.GetPawnByName(_pawnName);
            if (pawn == null) {
                return false; // Not a colonist nor a prisoner
            }

            switch (_condition) {
                case PawnCondition.Colonist:
                    return pawn.IsColonist;
                case PawnCondition.Prisoner:
                    return pawn.IsPrisoner;
                default:
                    return false;
            }
        }

        public override string ToString() {
            return $"PawnName: {_pawnName}, Condition: {_condition}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _pawnName, "pawnName");
            Scribe_Values.Look(ref _condition, "condition");
        }
        
        public enum PawnCondition {
            Colonist,
            Prisoner
        }
    }
}