using System;
using System.Collections.Generic;
using HumanStoryteller.Web;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Views; 
public class MainTabWindow_RateTab : MainTabWindow {
    public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;
    public override Vector2 RequestedTabSize =>  new Vector2(596, 240);

    private int _rating = -2;
    private bool _refreshing;

    private static Dictionary<HumanStoryteller.RefreshRate, string> _labels =
        new Dictionary<HumanStoryteller.RefreshRate, string> {
            {HumanStoryteller.RefreshRate.Short, "ShortDuration".Translate()},
            {HumanStoryteller.RefreshRate.Medium, "MediumDuration".Translate()},
            {HumanStoryteller.RefreshRate.Long, "LongDuration".Translate()},
            {HumanStoryteller.RefreshRate.Off, "OffDuration".Translate()}
        };

    public override void DoWindowContents(Rect inRect) {
        Text.Font = GameFont.Small;
        RenderRating(inRect);
    }

    private void RenderRating(Rect inRect) {
        if (_rating < 0 && !_refreshing) {
            _refreshing = true;
            RefreshRating();
        }

        Rect textRect = new Rect(inRect.x, inRect.y, inRect.width, 50);
        if (HumanStoryteller.IsNoStory) {
            Widgets.Label(textRect, "NoStorySadFace".Translate());
            return;
        }

        string ratingMessage;
        if (_rating == -1) {
            ratingMessage = "NotVoted".Translate();
        } else if (_rating == -2){
            ratingMessage = "LoadingVote".Translate();
        } else {
            ratingMessage = "Voted".Translate() + ": " + (_rating + 1);
        }
        Widgets.Label(textRect, $"{"RateStory".Translate()}:\n({ratingMessage})");

        for (int i = 0; i < 10; i++) {
            Rect buttonRect = new Rect(inRect.x + 30 + i * 50, inRect.y + 50, 50, 50);
            if (Widgets.ButtonText(buttonRect, (i + 1).ToString())) {
                _rating = -2;
                Storybook.SetRating(HumanStoryteller.StoryId, i, ResetRefresh);
            }
        }

        Rect switchLabelRect = new Rect(inRect.x, inRect.y + 110, inRect.width, 90);
        Widgets.Label(switchLabelRect, "SyncRate".Translate());
        var rates = (HumanStoryteller.RefreshRate[]) Enum.GetValues(typeof(HumanStoryteller.RefreshRate));
        for (var i = 0; i < rates.Length; i++) {
            Rect switchRect = new Rect(inRect.x + 50 + 130 * i, inRect.y + 150, 80, 50);
            if (Widgets.RadioButtonLabeled(switchRect, _labels[rates[i]], HumanStoryteller.StoryComponent.ThreatCycle.CurrentRate == rates[i])) {
                HumanStoryteller.StoryComponent.ThreatCycle.SetRefreshRate(rates[i]);
            }
        }
    }

    private void ResetRefresh() {
        _refreshing = false;
    }

    private void RefreshRating() {
        Storybook.GetRating(HumanStoryteller.StoryId, i => { _rating = i; });
    }
}