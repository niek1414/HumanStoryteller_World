using System;
using System.Linq;
using System.Threading;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using Verse;

namespace HumanStoryteller {
    public class HumanStoryteller : Mod {
        public static bool InitiateEventUnsafe = false;


        public HumanStoryteller(ModContentPack content) : base(content) {
            Thread humanStoryThread = new Thread(SwitchStoryteller);
            humanStoryThread.Start();
        }

        private void SwitchStoryteller() {
            while (true) {
                if (Current.Game != null) {
                    if (Find.TickManager.TicksGame > 1) {
                        //TODO REMOVE !!TEST!!
                        StorytellerDef storyteller = (from d in DefDatabase<StorytellerDef>.AllDefs
                            where d.defName.Contains("Human")
                            select d).First();
                        Tell.Warn("SWITCHED TO TEST STORYTELLER: " + storyteller.defName);
                        Current.Game.storyteller.def = storyteller;
                        Current.Game.storyteller.Notify_DefChanged();

//                        String str = "";
//                        foreach (PawnKindDef pawnKindDef in (from x in DefDatabase<PawnKindDef>.AllDefs
//                            where x.RaceProps.Animal
//                            select x)) {
//                            str += pawnKindDef.defName + "\n";
//                        }
//                        Tell.Log(str);
//
//                        String str = "";
//                        foreach (var i in (from d in DefDatabase<FactionDef>.AllDefs select d)) {
//                            str += i.defName + "\n";
//                        }
//                        Tell.Log(str);
                        return;
                    }
                }
            }
        }

        public static void RefreshStory(Action<Story> getStoryCallback) {
            Storybook.GetStory(24, getStoryCallback);
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