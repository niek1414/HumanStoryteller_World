using System;
using System.Collections.Generic;
using HumanStoryteller.Util;

namespace HumanStoryteller.IntegrityTest; 
public abstract class TestController {
    private static string TestDir = OSUtil.GetAssemblyPath() + "Tests/";

    private static List<TestModule> Tests = new List<TestModule> {
        new StoryLoadingModule(TestDir + "STORYLINE_simple")
    };

    public static Exception ExecuteAllTests() {
        try {
            foreach (var module in Tests) {
                module.Run();
            }
        } catch (Exception e) {
            return e;
        }

        return null;
    }
}