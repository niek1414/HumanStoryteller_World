using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HumanStoryteller.Model.StoryPart;
using UnityEngine.Assertions;

namespace HumanStoryteller.IntegrityTest {
    public class StoryLoadingModule : TestModule {
        private readonly string _path;

        public StoryLoadingModule(string path) {
            _path = path;
        }

        public void Run() {
            if (!File.Exists(_path + ".json")) throw new FileNotFoundException("Test with path " + _path + ".json not found");
            var storyArc = Parser.Parser.StoryParser(File.ReadAllText(_path + ".json"));
            
            if (!File.Exists(_path + ".test")) throw new FileNotFoundException("Test with path " + _path + ".test not found");
            foreach (var row in File.ReadAllLines(_path + ".test")) {
                var key = row.Split('=')[0];
                var value = string.Join("=", row.Split('=').Skip(1).ToArray());
                ExecuteCheck(key, value, storyArc);
            }
        }

        private static void ExecuteCheck(string key, string value, StoryArc arc) {
            switch (key) {
                case "STORY_AMOUNT":
                    var i = 0;
                    if (arc.LongStoryController.HasLongStory()) {
                        i++;
                    }

                    i += arc.ShortStoryController.CountOfInitialStories();
                    if (i != int.Parse(value)) {
                        ThrowOnCheckFail("STORY_AMOUNT", "Should be " + value + " but is " + i);
                    }

                    break;
                default:
                    throw new ArgumentException("Check not known: " + key);
            }
        }

        private static void ThrowOnCheckFail(string checkName, string message) {
            throw new AssertionException($"Check {checkName} failed: {message}", null);
        }
    }
}