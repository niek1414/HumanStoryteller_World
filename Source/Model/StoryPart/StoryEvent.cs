using Verse;

namespace HumanStoryteller.Model.StoryPart {
    public class StoryEvent : IExposable {
        public string Uuid;
        public string Name;
        public FiringHumanIncident Incident;

        public StoryEvent() {
        }

        public StoryEvent(string uuid, string name, FiringHumanIncident incident) {
            Uuid = uuid;
            Name = name;
            Incident = incident;
        }

        public override string ToString() {
            return $"Uuid: {Uuid}, Name: {Name}, Incident: {Incident}";
        }

        public void ExposeData() {
            Scribe_Deep.Look(ref Incident, "incident");
            Scribe_Values.Look(ref Uuid, "uuid");
        }
    }
}