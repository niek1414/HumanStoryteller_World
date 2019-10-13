using System.Collections.Generic;
using HumanStoryteller.Util;
using RimWorld;

namespace HumanStoryteller.Helper.QuestHelper {
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