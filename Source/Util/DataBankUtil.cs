using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Util {
    public class DataBankUtil {
        public static void ProcessVariableModifications(List<VariableModifications> mods) {
            if (mods == null || mods.Count < 1) return;
            Dictionary<string, float> variableBank = HumanStoryteller.StoryComponent.VariableBank;
            foreach (var mod in mods) {
                if (!variableBank.ContainsKey(mod.Name)) {
                    variableBank.Add(mod.Name, 0);
                }

                float num = mod.Constant.GetValue();
                switch (mod.Modification) {
                    case ModificationType.Add:
                        variableBank[mod.Name] += num;
                        break;
                    case ModificationType.Subtract:
                        variableBank[mod.Name] -= num;
                        break;
                    case ModificationType.Divide:
                        if (num == 0) return;
                        variableBank[mod.Name] /= num;
                        break;
                    case ModificationType.Multiply:
                        variableBank[mod.Name] *= num;
                        break;
                    case ModificationType.Equal:
                        variableBank[mod.Name] = num;
                        break;
                    default:
                        Tell.Err("Variable modification type not present or known");
                        break;
                }
            }
        }

        public static string VariableBankToString() {
            var result = "";
            foreach (var pair in HumanStoryteller.StoryComponent.VariableBank) {
                result += $"{pair.Key} = {pair.Value}\n";
            }

            return result;
        }

        public static bool CompareVariableWithConst(string variable, CompareType type, float constant) {
            return CompareValueWithConst(GetValueFromVariable(variable), type, constant);
        }

        public static float GetValueFromVariable(string variable) {
            switch (variable) {
                case "_DAYS":
                    return GetDaysPassed();
                case "_TREAT_POINTS":
                    return GetThreatPoints();
                case "_WEALTH":
                    return GetWealth();
            }

            Dictionary<string, float> variableBank = HumanStoryteller.StoryComponent.VariableBank;

            if (!variableBank.ContainsKey(variable)) {
                variableBank.Add(variable, 0);
            }

            return variableBank[variable];
        }

        public static int GetDaysPassed() {
            return GenDate.DaysPassed;
        }

        public static float GetThreatPoints() {
            return StorytellerUtility.DefaultThreatPointsNow(HumanStoryteller.StoryComponent.SameAsLastEvent);
        }

        public static float GetWealth() {
            float total = 0;
            Find.Maps.FindAll(x => x.ParentFaction.IsPlayer).ForEach(m => total += m.wealthWatcher.WealthTotal);
            return total;
        }

        public static bool CompareValueWithConst(float value, CompareType type, float constant) {
            bool result = false;
            string compType = "";
            switch (type) {
                case CompareType.Less:
                    result = value < constant;
                    compType = "less then";
                    break;
                case CompareType.More:
                    result = value > constant;
                    compType = "more then";
                    break;
                case CompareType.Equal:
                    result = Math.Abs(value - constant) < 0.001;
                    compType = "equal to";
                    break;
                default:
                    Tell.Err("Variable compare type not present or known", type);
                    compType = "UNKNOWN EVALUATION";
                    break;
            }

            Tell.Log("Compare -- is " + value + " " + compType + " " + constant + "? " + (result ? "YES" : "NO"));
            return result;
        }

        public static readonly Dictionary<string, CompareType> compareDict = new Dictionary<string, CompareType> {
            {"Less", CompareType.Less},
            {"More", CompareType.More},
            {"Equal", CompareType.Equal}
        };

        public enum CompareType {
            Less,
            More,
            Equal
        }
    }
}