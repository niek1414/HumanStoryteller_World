using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HumanStoryteller.Patch {
    public class StorytellerUI_Patch {
        private const int PAGE_STORY_LIMIT = 5;

        private static int _pageNumber;
        private static StorySummary _selectedSummary;
        private static bool _loading;
        private static float _loadingState;

        private static List<StorySummary> _storyList = new List<StorySummary>();
        private static Dictionary<string, WWW> _loadingImages = new Dictionary<string, WWW>();
        private static Dictionary<string, Texture2D> _loadedImages = new Dictionary<string, Texture2D>();
        private static Dictionary<long, Vector2> _descriptionScrollList = new Dictionary<long, Vector2>();

        private static string _filterName = "";
        private static string _filterCreator = "";
        private static string _filterDescription = "";

        public static void reset() {
            _pageNumber = 0;
            _selectedSummary = null;
            _loading = false;

            _filterName = "";
            _filterCreator = "";
            _filterDescription = "";

            _storyList.Clear();
            _loadingImages.Clear();
            _loadedImages.Clear();
            _descriptionScrollList.Clear();
        }


        public static void Patch(HarmonyInstance harmony) {
            MethodInfo targetMain = AccessTools.Method(typeof(StorytellerUI), "DrawStorytellerSelectionInterface");
            MethodInfo targetPre = AccessTools.Method(typeof(Page_SelectStoryteller), "PreOpen");
            MethodInfo targetPost = AccessTools.Method(typeof(Page_SelectStoryteller), "CanDoNext");

            HarmonyMethod draw = new HarmonyMethod(typeof(StorytellerUI_Patch).GetMethod("DrawStorytellerSelectionInterface"));
            HarmonyMethod pre = new HarmonyMethod(typeof(StorytellerUI_Patch).GetMethod("PreOpen"));
            HarmonyMethod post = new HarmonyMethod(typeof(StorytellerUI_Patch).GetMethod("CanDoNext"));

            harmony.Patch(targetMain, null, draw);
            harmony.Patch(targetPre, null, pre);
            harmony.Patch(targetPost, null, post);
        }

        public static void PreOpen() {
            reset();
            RefreshList(false);
        }

        public static void CanDoNext(Page_SelectStoryteller __instance, ref bool __result) {
            try {
                if (!__result || Traverse.Create(__instance).Field("storyteller").GetValue<StorytellerDef>().defName != "Human") return;
                if (_selectedSummary == null) {
                    __result = false;
                    return;
                }

                HumanStoryteller.StoryComponent.StoryId = (int) _selectedSummary.Id;
                LongEventHandler.QueueLongEvent(
                    delegate { HumanStoryteller.GetStoryCallback(Storybook.GetStory(HumanStoryteller.StoryComponent.StoryId)); }, "LoadingStory",
                    true, ErrorWhileLoadingStory);
            } finally {
                if (__result) {
                    reset();
                }
            }
        }

        public static void ErrorWhileLoadingStory(Exception e) {
            Tell.Err("Failed to load story, " + e.Message + " __TRACE__ " + e.StackTrace);
            string text = "LoadingStoryFailed".Translate() + "\n\nError:" + e.Message + " More info in console ";
            string loadedModsSummary;
            string runningModsSummary;
            if (!ScribeMetaHeaderUtility.LoadedModsMatchesActiveMods(out loadedModsSummary, out runningModsSummary))
                text = text + "\n\n" + "ModsMismatchWarningText".Translate((NamedArgument) loadedModsSummary, (NamedArgument) runningModsSummary);
            DelayedErrorWindowRequest.Add(text, "ErrorWhileLoadingMapTitle".Translate());
            Scribe.ForceStop();
            GenScene.GoToMainMenu();
        }

        public static void DrawStorytellerSelectionInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty,
            Listing_Standard infoListing) {
            if (chosenStoryteller.defName != "Human") return;

            Rect filter = new Rect(rect.x + 140, rect.y + 440, 300, 190);
            DrawFilter(filter);

            Rect stories = new Rect(rect.x + 450, rect.y, 530, 630);
            DrawStoryList(stories);
            DrawPagination(stories);
        }

        private static void DrawFilter(Rect filter) {
            Widgets.Label(new Rect(filter.x, filter.y, filter.width, 30), "StoryTitleInput".Translate());
            _filterName = Widgets.TextField(new Rect(filter.x, filter.y + 20, filter.width, 30), _filterName, 255, new Regex(".*"));

            Widgets.Label(new Rect(filter.x, filter.y + 60, filter.width, 30), "StoryDescriptionInput".Translate());
            _filterDescription = Widgets.TextField(new Rect(filter.x, filter.y + 80, filter.width, 30), _filterDescription, 255, new Regex(".*"));

            Widgets.Label(new Rect(filter.x, filter.y + 120, filter.width, 30), "StoryCreatorInput".Translate());
            _filterCreator = Widgets.TextField(new Rect(filter.x, filter.y + 140, filter.width / 3 * 2, 30), _filterCreator, 255, new Regex(".*"));

            if (Widgets.ButtonText(new Rect(filter.x + filter.width / 3 * 2, filter.y + 140, filter.width / 3, 30), "Search")) {
                _pageNumber = 0;
                RefreshList(false);
            }
        }

        private static void DrawPagination(Rect stories) {
            if (Widgets.ButtonText(new Rect(stories.center.x - 150, stories.yMax - 40, 90, 40), "Random 5")) {
                if (!_loading) {
                    _pageNumber = 0;
                    RefreshListRandom();
                }
            }

            if (Widgets.ButtonText(new Rect(stories.center.x - 40, stories.yMax - 40, 30, 40), "<", true, false, _pageNumber != 0)) {
                if (!_loading && _pageNumber != 0) {
                    _pageNumber--;
                    RefreshList();
                }
            }

            string label;
            if (_loading) {
                _loadingState += 0.1f;
                switch (Math.Round(_loadingState % 6)) {
                    case 0:
                        label = "☰";
                        break;
                    case 1:
                        label = "☱";
                        break;
                    case 2:
                        label = "☳";
                        break;
                    case 3:
                        label = "☷";
                        break;
                    case 4:
                        label = "☶";
                        break;
                    case 5:
                        label = "☴";
                        break;
                    default:
                        label = "☰";
                        break;
                }
            } else {
                label = _pageNumber.ToString();
            }

            Widgets.Label(new Rect(stories.center.x + 1, stories.yMax - 27, 20, 30), label);
            if (Widgets.ButtonText(new Rect(stories.center.x + 20, stories.yMax - 40, 30, 40), ">")) {
                if (!_loading) {
                    _pageNumber++;
                    RefreshList();
                }
            }

            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(stories.xMax - 90, stories.yMax - 14, 90, 17),
                "Picked story: #" + (_selectedSummary == null ? "-" : _selectedSummary.Id.ToString()));
        }

        private static void DrawStoryList(Rect stories) {
            Listing_Standard listing = new Listing_Standard {ColumnWidth = stories.width};
            listing.Begin(stories);
            Text.Font = GameFont.Small;
            listing.Label("StoryList".Translate());
            ListStories(listing, _storyList);
            listing.End();
        }

        private static void ListStories(Listing_Standard listing, List<StorySummary> stories) {
            bool flag = false;
            for (var i = 0; i < stories.Count; i++) {
                StorySummary story;
                try {
                    story = stories[i];
                } catch (ArgumentOutOfRangeException e) {
                    continue;
                }

                if (flag) {
                    listing.Gap();
                } else if (_selectedSummary == null) {
                    _selectedSummary = story;
                }

                Rect rect = listing.GetRect(100);
                DrawStorySummary(rect, story);
                flag = true;
            }

            if (!flag) {
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
                listing.Label("(" + "NoneLower".Translate() + ")");
                GUI.color = Color.white;
            }
        }

        private static void DrawStorySummary(Rect rect, StorySummary story) {
            bool selected = _selectedSummary != null && _selectedSummary.Id == story.Id;
            Widgets.DrawOptionBackground(rect, selected);
            MouseoverSounds.DoRegion(rect);

            if (story.Featured) {
                Widgets.DrawShadowAround(rect);
                Color color1 = GUI.color;
                GUI.color = new Color(0.39f, 0.33f, 0.02f);
                Widgets.DrawBox(rect);
                GUI.color = color1;
            }

            Rect inner = rect.ContractedBy(4f);
            Text.Font = GameFont.Small;
            Rect title = inner;
            title.height = Text.CalcHeight(story.Name, title.width);
            Widgets.Label(title, story.Name);

            Text.Font = GameFont.Tiny;
            Rect description = new Rect(inner) {width = inner.width - 90};
            description.y += title.height;
            description.height -= title.height * 2;

            if (!_descriptionScrollList.ContainsKey(story.Id)) {
                _descriptionScrollList.Add(story.Id, new Vector2());
            }

            Vector2 scrollLocation = _descriptionScrollList[story.Id];
            Widgets.LabelScrollable(description, story.Description, ref scrollLocation, false, false);
            _descriptionScrollList[story.Id] = scrollLocation;

            Rect rating = new Rect(title);
            rating.y += description.height + rating.height;
            GUIStyle alignCenter = Text.CurFontStyle;
            alignCenter.alignment = TextAnchor.LowerCenter;
            GUI.Label(rating, $"Rating: {story.Rating} / Votes: {story.Votes}", alignCenter);

            Rect avatar = new Rect(inner.xMax - 65, inner.y + 18, 55, 55);

            Texture2D foundImage = GetImage(story.Avatar);
            if (foundImage != null) {
                GUI.DrawTexture(avatar, foundImage, ScaleMode.ScaleToFit);
            }

            if (!selected && Widgets.ButtonInvisible(rect)) {
                _selectedSummary = story;
                if (HumanStoryteller.StoryComponent.Initialised) {
                    HumanStoryteller.StoryComponent.StoryId = story.Id;
                    HumanStoryteller.StoryComponent.ForcedUpdate = true;
                }
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        private static Texture2D GetImage(string url) {
            if (_loadingImages.ContainsKey(url)) {
                WWW val = _loadingImages[url];
                try {
                    if (val.isDone) {
                        if (val.error == null) {
                            _loadedImages.Add(url, val.textureNonReadable);
                            _loadingImages.Remove(url);
                        } else {
                            Tell.Warn("Could not load avatar", "url: " + url, "Err: " + val.error);
                            _loadingImages.Remove(url);
                        }

                        val.Dispose();
                    }
                } catch (Exception e) {
                    try {
                        Tell.Warn("Could not load avatar (disposing now)", "url: " + url, "Err: " + e.Message + " Stack__ " + e.StackTrace);
                        val.Dispose();
                        _loadingImages.Remove(url);
                    } catch (Exception) {
                        // ignored
                    }
                }
            }

            if (_loadedImages.ContainsKey(url)) {
                return _loadedImages[url];
            }

            if (!_loadingImages.ContainsKey(url)) {
                _loadingImages.Add(url, new WWW(GenFilePaths.SafeURIForUnityWWWFromPath(url).Substring(8)));
            }

            return null;
        }

        private static void RefreshList(bool autoFind = true) {
            _loading = true;
            long origin = _pageNumber * PAGE_STORY_LIMIT;
            Storybook.GetBook(origin, PAGE_STORY_LIMIT, _filterName, _filterDescription, _filterCreator, storyArray => {
                _loading = false;

                if (storyArray.Length > 0) {
                    _storyList.Clear();
                    _storyList.AddRange(storyArray);
                } else if (autoFind) {
                    _pageNumber = 0;
                    RefreshList(false);
                } else {
                    _storyList.Clear();
                }
            });
        }

        private static void RefreshListRandom() {
            _loading = true;
            Storybook.GetBookRandom(storyArray => {
                _loading = false;

                if (storyArray.Length > 0) {
                    _storyList.Clear();
                    _storyList.AddRange(storyArray);
                } else {
                    _pageNumber = 0;
                    RefreshList(false);
                }
            });
        }
    }
}