using System;
using Verse;

namespace HumanStoryteller.Util {
    public class PawnUtil {
        public static Pawn GetPawnByName(String name) {
            foreach (Map map in Find.Maps) {
                foreach (Pawn p in map.mapPawns.AllPawns) {
                    if (p?.Name == null) {
                        continue;
                    }

                    if (p.Name.ToStringFull.Contains(name)) {
                        return p;
                    }
                }
            }

            return null;
        }
    }
}