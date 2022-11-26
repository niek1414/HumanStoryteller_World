using System;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay; 
public class ChapterBar : IOverlayItem {
    private const int FadeInTime = 3;
    private const int FadeOutTime = 1;
    private const int VisibleTime = 7;

    private const float MaxOpacity = 0.58f;

    private enum State {
        FadeIn,
        Visible,
        FadeOut
    }

    private float _timeSinceLastTransition;
    private State _currentState = State.FadeIn;

    private string _title;
    private string _description;

    public ChapterBar() {
    }

    public ChapterBar(string title, string description) {
        _title = title;
        _description = description;
    }

    public bool Step() {
        _timeSinceLastTransition += Time.deltaTime;
        float alpha;
        float textAlpha;
        float textOffset;

        switch (_currentState) {
            case State.FadeIn:
                if (_timeSinceLastTransition >= FadeInTime) {
                    SetState(State.Visible);
                    alpha = MaxOpacity;
                    textAlpha = 1f;
                    textOffset = 0;
                } else {
                    float progress = (FadeInTime - _timeSinceLastTransition) / FadeInTime;
                    alpha = Mathf.Min(1 - progress, MaxOpacity);
                    textAlpha = 1f;
                    textOffset = 200 - (1 - progress * progress) * 200;
                }

                break;
            case State.Visible:
                if (_timeSinceLastTransition >= VisibleTime) {
                    SetState(State.FadeOut);
                }

                alpha = MaxOpacity;
                textAlpha = 1f;
                textOffset = 0;
                break;
            case State.FadeOut:
                if (_timeSinceLastTransition >= FadeOutTime) {
                    return true;
                }

                var outProgress = (FadeOutTime - _timeSinceLastTransition) / FadeOutTime;
                alpha = outProgress * MaxOpacity;
                textAlpha = outProgress;
                textOffset = -(1 - outProgress * outProgress) * 200;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Draw(textOffset, alpha, textAlpha);
        return false;
    }

    public void HighPrio() {
    }

    public void NotifyEnd() {
        SetState(State.FadeOut);
    }

    public bool ShouldBlockInput() {
        return false;
    }

    private void SetState(State state) {
        _currentState = state;
        _timeSinceLastTransition = 0;
    }

    private void Draw(float textOffset, float alpha, float textAlpha) {
        var barHeight = _title == "" || _description == "" ? 100 : 200;
        var outerRect = new Rect(0, UI.screenHeight / 4, UI.screenWidth, barHeight);
        var topRect = new Rect(outerRect) {height = 100, x = textOffset};
        var bottomRect = new Rect(outerRect) {y = outerRect.y + 100, height = 100, x = -textOffset};

        Widgets.DrawBoxSolid(outerRect, new Color(0.08f, 0.1f, 0.11f, alpha));
        Text.Font = GameFont.Medium;
        GUI.color = new Color(1f, 1f, 1f, textAlpha);

        var oldAnchor = Text.Anchor;
        Text.Anchor = TextAnchor.MiddleCenter;
        GUIStyle style = Text.CurFontStyle;
        var oldSize = style.fontSize;

        style.fontSize = _title != "" ? 60 : 30;
        GUI.Label(topRect, _title != "" ? _title : _description, Text.CurFontStyle);
        if (_title != "" && _description != "") {
            GUI.color = new Color(0.8f, 0.8f, 0.8f, alpha);
            Widgets.DrawLineHorizontal(UI.screenWidth * 0.1f, topRect.yMax, UI.screenWidth * 0.8f);
            GUI.color = new Color(0.8f, 0.8f, 0.8f, textAlpha);
            style.fontSize = 30;
            GUI.Label(bottomRect, _description, Text.CurFontStyle);
        }

        style.fontSize = oldSize;
        Text.Anchor = oldAnchor;
        GUI.color = new Color(1f, 1f, 1f, 1f);
    }

    public void ExposeData() {
        Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
        Scribe_Values.Look(ref _currentState, "currentState");
        Scribe_Values.Look(ref _title, "title");
        Scribe_Values.Look(ref _description, "description");
    }
}