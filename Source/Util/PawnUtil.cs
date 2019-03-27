using System;
using System.Collections.Generic;
using Verse;

namespace HumanStoryteller.Util {
    public class PawnUtil {
        public static Pawn GetPawnByName(String name) {
            var pawnBank = StorytellerComp_HumanThreatCycle.StoryComponent.PawnBank;
            foreach (var pair in pawnBank) {
                if (pair.Key.ToUpper().Equals(name.ToUpper())) {
                    return pair.Value;
                }
            }

            return null;
        }

        public static void SavePawnByName(String name, Pawn pawn) {
            StorytellerComp_HumanThreatCycle.StoryComponent.PawnBank.Add(name, pawn);
        }

        public static void RemoveName(string name) {
            StorytellerComp_HumanThreatCycle.StoryComponent.PawnBank.Remove(name);
        }

        public static Gender GetGender(string genderString) {
            switch (genderString) {
                case "MALE":
                    return Gender.Male;
                case "FEMALE":
                    return Gender.Female;
                default:
                    return Gender.None;
            }
        }

        public static bool PawnExists(Pawn pawn) {
            var pawnBank = StorytellerComp_HumanThreatCycle.StoryComponent.PawnBank;
            foreach (var pair in pawnBank) {
                if (pair.Value == pawn) {
                    return true;
                }
            }

            return false;
        }
    }
}