using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HumanStoryteller.Util.Logging;
using UnityEngine;
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

        public static Color? HexToColor(string hex) {
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");
 
            int red;
            int green;
            int blue;
            try {

                switch (hex.Length) {
                    case 6:
                        //#RRGGBB
                        red = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                        green = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                        blue = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                        return new Color(red / 255f, green / 255f, blue / 255f);
                    case 3:
                        //#RGB
                        red = int.Parse(hex[0].ToString() + hex[0], NumberStyles.AllowHexSpecifier);
                        green = int.Parse(hex[1].ToString() + hex[1], NumberStyles.AllowHexSpecifier);
                        blue = int.Parse(hex[2].ToString() + hex[2], NumberStyles.AllowHexSpecifier);
                        return new Color(red / 255f, green / 255f, blue / 255f);
                    default:
                        Tell.Warn("Color has a wrong amount of characters: " + hex);
                        break;
                }

            } catch (Exception e) {
                Tell.Warn("Color has a wrong format: " + hex, e);
            }
            return null;
        }
    }
}