using Harmony;
using Verse;

namespace HumanStoryteller {
    public class HumanStoryteller : Mod {
        public static bool InitiateEventUnsafe = false;


        public HumanStoryteller(ModContentPack content) : base(content) {
        }
    }

    [StaticConstructorOnStartup]
    static class HarmonyPatches {
        static HarmonyPatches() {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.keyboxsoftware.humanstoryteller");

            Patch.Main_Patch.Patch(harmony);
            Patch.StorytellerUI_Patch.Patch(harmony);
        }
    }
}