using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Util {
    public class PawnGroupUtil {
        private static int _cleanupCounter;
        private const int CleanupCounterMax = 100;

        public static PawnGroup GetGroupByName(String name) {
            var groupBank = HumanStoryteller.StoryComponent.PawnGroupBank;

            _cleanupCounter++;
            if (_cleanupCounter >= CleanupCounterMax) {
                _cleanupCounter = 0;
                foreach (var item in groupBank.Where(pair => pair.Value.Prune()).ToList()) {
                    Tell.Log("Removing group with name: " + item.Key);
                    groupBank.Remove(item.Key);
                }
            }

            foreach (var pair in groupBank) {
                if (pair.Key.ToUpper().Equals(name.ToUpper())) {
                    Tell.Log("Found group with name: " + name + " with " + pair.Value.Pawns.Count + " pawns.");
                    return pair.Value;
                }
            }

            Tell.Log("No group found with name: " + name);
            return null;
        }

        public static void SaveGroupByName(String name, PawnGroup group) {
            if (HumanStoryteller.StoryComponent.PawnGroupBank.ContainsKey(name)) {
                RemoveName(name);
            }
            
            Tell.Log("Saved group with " + group.Pawns.Count + " pawns as: " + name);
            HumanStoryteller.StoryComponent.PawnGroupBank.Add(name, group);
        }

        public static void RemoveName(string name) {
            Tell.Log("Removed group with name: " + name);
            HumanStoryteller.StoryComponent.PawnGroupBank.Remove(name);
        }
    }
}