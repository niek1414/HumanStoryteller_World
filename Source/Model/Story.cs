using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.Model {
    public class Story : IExposable {
        public Story(long id, string name, string description, long creator, StoryGraph storyGraph) {
            Id = id;
            Name = name;
            Description = description;
            Creator = creator;
            StoryGraph = Tell.AssertNotNull(storyGraph, nameof(storyGraph), GetType().Name);
        }

        public Story() {
        }

        public long Id;
        public string Name;
        public string Description;
        public long Creator;
        public StoryGraph StoryGraph;

        public override string ToString() {
            return $"Id: {Id}, Name: {Name}, Description: {Description}, Creator: {Creator}, StoryGraph: {StoryGraph}";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Id, "id");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref Description, "description");
            Scribe_Values.Look(ref Creator, "creator");
            Scribe_Deep.Look(ref StoryGraph, "storyGraph");
        }
    }
}