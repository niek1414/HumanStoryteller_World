using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Util {
    public class PawnUtil {

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
                    if (first != "" || nick != "" || last != "") {
                        p.Name = new NameSingle(first + " " + nick + " " + last);
                    }

                    break;
            }
        }

        public static Pawn GetPawnByName(String name) {
            var pawnBank = HumanStoryteller.StoryComponent.PawnBank;

            if (pawnBank.ContainsKey(name.ToUpper())) {
                var pawn = pawnBank[name.ToUpper()];
                Tell.Log("Found pawn with name I: " + name + " S: " + pawn.Name.ToStringShort);
                if (pawn.Discarded) {
                    RemoveName(name);
                } else {
                    return pawn;
                }
            }

            Tell.Log("No pawn found with name I: " + name);
            return null;
        }

        public static void SavePawnByName(String name, Pawn pawn) {
            if (HumanStoryteller.StoryComponent.PawnBank.ContainsKey(name.ToUpper())) {
                RemoveName(name);
            }

            Tell.Log("Saved pawn S: " + pawn.Name.ToStringShort + " as I: " + name);
            HumanStoryteller.StoryComponent.PawnBank.Add(name.ToUpper(), pawn);
        }

        public static void RemoveName(string name) {
            Tell.Log("Removing pawn with name I: " + name);
            HumanStoryteller.StoryComponent.PawnBank.Remove(name.ToUpper());
        }

        public static void RemovePawn(Pawn pawn) {
            Tell.Log("Removing pawn with name D: " + pawn.Name);
            foreach (var item in HumanStoryteller.StoryComponent.PawnBank.Where(kvp => kvp.Value == pawn).ToList()) {
                HumanStoryteller.StoryComponent.PawnBank.Remove(item.Key);
            }
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
            return HumanStoryteller.StoryComponent.PawnBank.Any(pair => pair.Value == pawn);
        }
    }
}