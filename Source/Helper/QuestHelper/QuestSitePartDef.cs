using System.Collections.Generic;
using RimWorld;

namespace HumanStoryteller.Helper.QuestHelper {
    public class QuestSitePartDef : SitePartDef {
        private readonly List<string> _names;

        public QuestSitePartDef(List<string> names) {
            defName = "QuestSitePartDef";
            _names = names;
            workerClass = typeof(QuestSitePartWorker);
        }

        public new SitePartWorker Worker => new QuestSitePartWorker(_names);
    }
}