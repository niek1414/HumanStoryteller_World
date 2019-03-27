using System;
using System.Collections.Generic;
using HumanStoryteller.Model;

namespace HumanStoryteller.Util {
    public class DataBank {
        public static void ProcessVariableModifications(List<VariableModifications> mods) {
            if (mods == null || mods.Count < 1) return;
            Dictionary<string, float> variableBank = StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank;
            foreach (var mod in mods) {
                if (!variableBank.ContainsKey(mod.Name)) {
                    variableBank.Add(mod.Name, 0);
                }

                switch (mod.Modification) {
                    case ModificationType.Add:
                        variableBank[mod.Name] += mod.Constant;
                        break;
                    case ModificationType.Subtract:
                        variableBank[mod.Name] -= mod.Constant;
                        break;
                    case ModificationType.Divide:
                        if (mod.Constant == 0) return;
                        variableBank[mod.Name] /= mod.Constant;
                        break;
                    case ModificationType.Multiply:
                        variableBank[mod.Name] *= mod.Constant;
                        break;
                    case ModificationType.Equal:
                        variableBank[mod.Name] = mod.Constant;
                        break;
                    default:
                        Tell.Err("Variable modification type not present or known");
                        break;
                }
            }
        }

        public static string VariableBankToString() {
            var result = "";
            foreach (var pair in StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank) {
                result += $"{pair.Key} = {pair.Value}\n";
            }

            return result;
        }

        public static bool CompareVariableWithConst(string variable, CompareType type, float constant) {
            return CompareValueWithConst(GetValueFromVariable(variable), type, constant);
        }

        public static float GetValueFromVariable(string variable) {
            Dictionary<string, float> variableBank = StorytellerComp_HumanThreatCycle.StoryComponent.VariableBank;

            if (!variableBank.ContainsKey(variable)) {
                variableBank.Add(variable, 0);
            }

            return variableBank[variable];
        }

        public static bool CompareValueWithConst(float value, CompareType type, float constant) {
            switch (type) {
                case CompareType.Less:
                    return value < constant;
                case CompareType.More:
                    return value > constant;
                case CompareType.Equal:
                    return Math.Abs(value - constant) < 0.001;
                default:
                    Tell.Err("Variable compare type not present or known", type);
                    return false;
            }
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