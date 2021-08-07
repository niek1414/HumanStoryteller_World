using System.Collections.Generic;
using HumanStoryteller.NewtonsoftShell.Newtonsoft.Json;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Model.StoryPart {
    public class StoryArc : IExposable {
        public long Id;
        public string Name;
        public string Description;
        public long Creator;
        public LongStoryController LongStoryController = new LongStoryController();
        public ShortStoryController ShortStoryController = new ShortStoryController();

        public StoryArc() {
        }
        
        [JsonConstructor]
        public StoryArc(long id, string name, string description, long creator, List<StoryGraph> storyGraphs) {
            Id = id;
            Name = name;
            Description = description;
            Creator = creator;
            Tell.AssertNotNull(storyGraphs, nameof(storyGraphs), GetType().Name);
            foreach (var storyGraph in storyGraphs) {
                if (storyGraph.IsLongStory()) {
                    LongStoryController.Story = (LongStoryGraph) storyGraph;
                } else if (storyGraph.IsShortStory()) {
                    ShortStoryController.AddStory((ShortStoryGraph) storyGraph);
                } else {
                    Tell.Err("Found a story that is nether classified as long or short. Likely a version mismatch");
                }
            }
        }

        public void Tick() {
            LongStoryController.Tick();
            ShortStoryController.Tick();
        }

        public override string ToString() {
            return
                $"Id: [{Id}], Name: [{Name}], Description: [{Description}], Creator: [{Creator}], LongStoryController: [{LongStoryController}, ShortStoryController: [{ShortStoryController}]";
        }

        public void ExposeData() {
            Scribe_Values.Look(ref Id, "id");
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref Description, "description");
            Scribe_Values.Look(ref Creator, "creator");
            Scribe_Deep.Look(ref LongStoryController, "longStoryController");
            Scribe_Deep.Look(ref ShortStoryController, "shortStoryController");
        }
    }
}