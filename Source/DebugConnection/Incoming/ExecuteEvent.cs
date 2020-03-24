using HumanStoryteller.Incidents;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.DebugConnection.Incoming {
    public class ExecuteEvent : IncomingMessage {
        public bool WithRunner;
        public string Uuid;

        public ExecuteEvent() : base(MessageType.LoadStory) {
        }

        public override string ToString() {
            return $"Name: [{nameof(ExecuteEvent)}] WithRunner: [{WithRunner}], Uuid: [{Uuid}]";
        }

        public override void Handle() {
            Tell.Log("Handling incoming message: " + ToString());
            if (Current.Game == null || HumanStoryteller.StoryComponent == null || HumanStoryteller.IsNoStory) {
                Tell.Warn("Ignoring..");
                return;
            }

            var sn = HumanStoryteller.StoryComponent.Story.StoryGraph.GetCurrentNode(Uuid);
            if (sn == null) {
                Tell.Warn("Unable to execute node with uuid " + Uuid + ", not found");
                return;
            }

            IncidentResult incidentResult = null;
            if (sn.StoryEvent?.Incident?.Worker != null) {
                var incident = sn.StoryEvent.Incident;
                incidentResult = incident.Worker.ExecuteIncident(incident.Parms);
                DataBankUtil.ProcessVariableModifications(sn.Modifications);
            } else {
                Tell.Warn("Unable to execute node with uuid " + Uuid + ", the incident that was not defined");
            }

            if (WithRunner) {
                HumanStoryteller.StoryComponent.CurrentNodes.Add(new StoryEventNode(sn, Find.TickManager.TicksGame / 600, incidentResult));
                DebugWebSocket.TryUpdateRunners();
            }
        }
    }
}