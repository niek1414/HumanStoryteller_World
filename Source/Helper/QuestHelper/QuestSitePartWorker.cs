using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Helper.QuestHelper {
    public class QuestSitePartWorker : SitePartWorker {
        private readonly List<string> _names;
        
        public QuestSitePartWorker(List<string> names) {
            _names = names;
        }

        public override void PostMapGenerate(Map map) {
            base.PostMapGenerate(map);
            var pawns = map.mapPawns.AllPawns.ToArray();
            for (var i = 0; i < pawns.Length; i++) {
                if (_names.Count <= i) {
                    continue;
                }
                
                PawnUtil.SavePawnByName(_names[i], pawns[i]);
            }
        }
    }
}