using HumanStoryteller.Incidents;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.StoryPart {
    public class FiringHumanIncident : IExposable {
        private HumanIncidentParams _params;
        private HumanIncidentWorker _worker;

        public FiringHumanIncident() {
        }

        public FiringHumanIncident(HumanIncidentWorker worker, HumanIncidentParams @params = null) {
            _worker = Tell.AssertNotNull(worker, nameof(worker), GetType().Name);
            if (@params != null)
                _params = @params;
        }

        public override string ToString() {
            string str = "";
            if (_worker != null)
                str += _worker.GetType().Name;
            if (_params != null)
                str += " " + _params;
            return str;
        }

        public HumanIncidentParams Params => _params;

        public HumanIncidentWorker Worker => _worker;
        
        public void ExposeData() {
            Scribe_Deep.Look(ref _params, "params");
            Scribe_Deep.Look(ref _worker, "worker");
        }
    }
}