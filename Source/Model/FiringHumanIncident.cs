using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Model {
    public class FiringHumanIncident : IExposable {
        private HumanIncidentParms _parms;
        private HumanIncidentWorker _worker;

        public FiringHumanIncident() {
        }

        public FiringHumanIncident(HumanIncidentWorker worker, HumanIncidentParms parms = null) {
            _worker = Tell.AssertNotNull(worker, nameof(worker), GetType().Name);
            if (parms != null)
                _parms = parms;
        }

        public override string ToString() {
            string str = "";
            if (_worker != null)
                str += _worker.GetType().Name;
            if (_parms != null)
                str += " " + _parms;
            return str;
        }

        public HumanIncidentParms Parms => _parms;

        public HumanIncidentWorker Worker => _worker;
        
        public void ExposeData() {
            Scribe_Deep.Look(ref _parms, "parms");
            Scribe_Deep.Look(ref _worker, "worker");
        }
    }
}