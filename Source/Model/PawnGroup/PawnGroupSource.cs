using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.PawnGroup {
    public class PawnGroupSource : IExposable {
        public GroupSource Type = GroupSource.AllOnMap;
        public List<string> Names = new List<string>();
        public List<string> Groups = new List<string>();

        public PawnGroupSource() {
        }

        public PawnGroup GetSource(Map map) {
            switch (Type) {
                case GroupSource.AllOnMap:
                    if (map == null) {
                        Tell.Warn("Map is Null while trying to get pawn group source!");
                        return new PawnGroup();
                    }
                    return new PawnGroup(map.mapPawns.AllPawns.ToList());
                case GroupSource.Pawns:
                    var group = new PawnGroup();
                    foreach (var name in Names) {
                        var pawn = PawnUtil.GetPawnByName(name);
                        if (!pawn.DestroyedOrNull() && !pawn.Dead) {
                            group.Add(pawn);
                        }
                    }

                    return group;
                case GroupSource.PawnGroup:
                    var groups = new PawnGroup();
                    Groups.ForEach(g => groups.Add(PawnGroupUtil.GetGroupByName(g)));
                    return groups;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString() {
            return $"Type: {Type}, Names: {Names}, Group: {Groups}";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Type, "type");
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Collections.Look(ref Groups, "groups", LookMode.Value);
        }
    }
    
    public enum GroupSource {
        AllOnMap,
        Pawns,
        PawnGroup
    }
}