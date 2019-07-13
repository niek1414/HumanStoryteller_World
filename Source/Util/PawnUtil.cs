using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace HumanStoryteller.Util {
    public class PawnUtil {
        private static int _cleanupCounter;
        private const int CleanupCounterMax = 100;

        public static void SetDisplayName(Pawn p, string first, string nick = "", string last = "") {
            switch (p.Name) {
                case NameTriple n:
                    p.Name = new NameTriple(
                        first != "" ? first : n.First,
                        nick != "" ? nick : n.Nick,
                        last != "" ? last : n.Last
                    );
                    break;
                default:
                    if (first != "" || nick != "" || last != ""){
                        p.Name = new NameSingle(first + " " + nick + " " + last);
                    }
                    break;
            }
        }
        
        public static Pawn GetPawnByName(String name) {
            var pawnBank = HumanStoryteller.StoryComponent.PawnBank;

            _cleanupCounter++;
            if (_cleanupCounter >= CleanupCounterMax) {
                _cleanupCounter = 0;
                foreach (var item in pawnBank.Where(pair =>
                    pair.Value == null || pair.Value.Destroyed).ToList()) {
                    pawnBank.Remove(item.Key);
                }
            }

            foreach (var pair in pawnBank) {
                if (pair.Key.ToUpper().Equals(name.ToUpper())) {
                    return pair.Value;
                }
            }

            return null;
        }

        public static void SavePawnByName(String name, Pawn pawn) {
            if (HumanStoryteller.StoryComponent.PawnBank.ContainsKey(name)) {
                RemoveName(name);
            }

            HumanStoryteller.StoryComponent.PawnBank.Add(name, pawn);
        }

        public static void RemoveName(string name) {
            HumanStoryteller.StoryComponent.PawnBank.Remove(name);
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
            var pawnBank = HumanStoryteller.StoryComponent.PawnBank;
            foreach (var pair in pawnBank) {
                if (pair.Value == pawn) {
                    return true;
                }
            }

            return false;
        }
    }
}