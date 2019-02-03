using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Harmony;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace HumanStoryteller.Patch {
    public class StorytellerUI_Patch {
        private static int _pageNumber = 0;
        private static StorySummary _selectedSummary;
        private static bool _loading = true;
        private static float _loadingState;
        private static List<StorySummary> storyList = new List<StorySummary>();
        private static List<long> loadingImages = new List<long>();
        private static Dictionary<long, Texture2D> loadedImages = new Dictionary<long, Texture2D>();

        public static void Patch(HarmonyInstance harmony) {
            MethodInfo target = AccessTools.Method(typeof(StorytellerUI), "DrawStorytellerSelectionInterface");

            HarmonyMethod tick = new HarmonyMethod(typeof(StorytellerUI_Patch).GetMethod("DrawStorytellerSelectionInterface"));
            harmony.Patch(target, null, tick);
        }

        public static void DrawStorytellerSelectionInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty,
            Listing_Standard infoListing) {
            if (chosenStoryteller.defName != "Human") return;
            Rect stories = new Rect(rect.x + 450, rect.y, 530, 630);
            Listing_Standard listing = new Listing_Standard();
            listing.ColumnWidth = stories.width;
            listing.Begin(stories);
            Text.Font = GameFont.Small;
            listing.Label("FeaturedStories".Translate());
            ListStories(listing, storyList);
            listing.End();
            if (Widgets.ButtonText(new Rect(stories.center.x - 40, stories.yMax - 40, 30, 40), "<", true, false, _pageNumber != 0)) {
                _loading = true;
                Storybook.GetBook(storyArray => {
                    _loading = false;
                    storyList.Clear();
                    storyList.AddRange(storyArray);
                });
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
                _loading = true;
                Storybook.GetBook(storyArray => {
                    _loading = false;
                    storyList.Clear();
                    storyList.AddRange(storyArray);
                });
            }
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

                Rect rect = listing.GetRect(80);
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
            bool selected = _selectedSummary.Id == story.Id;
            Widgets.DrawOptionBackground(rect, selected);
            MouseoverSounds.DoRegion(rect);

            Rect inner = rect.ContractedBy(4f);
            Text.Font = GameFont.Small;
            Rect title = inner;
            title.height = Text.CalcHeight(story.Name, title.width);
            Widgets.Label(title, story.Name);

            Text.Font = GameFont.Tiny;
            Rect description = inner;
            description.width = inner.width - 90;
            description.y += title.height;
            description.height -= title.height;
            Widgets.Label(description, story.Description);

            Rect avatar = new Rect(inner.xMax - 65, inner.y + 7, 55, 55);

            Texture2D foundImage = GetImage(story.Avatar, story.Id);
            if (foundImage != null) {
                GUI.DrawTexture(avatar, foundImage, ScaleMode.ScaleToFit);
            }

            if (!selected && Widgets.ButtonInvisible(rect)) {
                _selectedSummary = story;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
        }

        private static Texture2D GetImage(string url, long id) {
            if (loadingImages.Contains(id)) {
                return null;
            }

            Texture2D image;
            try {
                image = loadedImages[id];
            } catch (Exception e) {
                image = null;
            }

            if (image != null) {
                return image;
            }

            loadingImages.Add(id);
            LongEventHandler.ExecuteWhenFinished(() => {
                string text = GenFilePaths.SafeURIForUnityWWWFromPath(url);
                WWW val = new WWW(text.Substring(8));
                try {
                    val.threadPriority = ThreadPriority.High;

                    while (!val.isDone) {
                        Thread.Sleep(1);
                    }

                    if (val.error == null) {
                        loadedImages.Add(id, val.textureNonReadable);
                        loadingImages.Remove(id);
                    } else {
                        Tell.Warn("Could not load avatar", "storyid: " + id, "Err: " + val.error);
                        loadingImages.Remove(id);
                    }
                } finally {
                    val.Dispose();
                }
            });
            return null;
        }
    }
}