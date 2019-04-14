using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HumanStoryteller.Util {
    public class QuestSitePartDef : SitePartDef {
        private readonly List<string> _names;

        public QuestSitePartDef(List<string> names) {
            defName = "QuestSitePartDef";
            _names = names;
            workerClass = typeof(QuestSitePartWorker);
        }

        public new SiteCoreOrPartWorkerBase Worker => new QuestSitePartWorker(_names);
        
        protected override SiteCoreOrPartWorkerBase CreateWorker() {
            return new QuestSitePartWorker(_names);
        }
    }
}