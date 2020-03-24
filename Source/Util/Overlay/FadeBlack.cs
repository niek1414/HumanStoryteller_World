using System;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class FadeBlack : IOverlayItem {
        private readonly bool _showLoading;
        private const int EnterTime = 3;
        private const int LeaveTime = 3;

        private enum State {
            FadeIn,
            Visible,
            FadeOut
        }

        private float _timeSinceLastTransition;
        private State _currentState = State.FadeIn;

        public FadeBlack() {
        }

        public FadeBlack(bool showLoading) {
            _showLoading = showLoading;
        }


        public bool Step() {
            _timeSinceLastTransition += Time.deltaTime;

            float alpha;

            switch (_currentState) {
                case State.FadeIn:
                    if (_timeSinceLastTransition >= EnterTime) {
                        SetState(State.Visible);
                        alpha = 1;
                    } else {
                        var inProgress = (EnterTime - _timeSinceLastTransition) / EnterTime;
                        alpha = (1 - inProgress) * 1f;
                    }

                    break;
                case State.Visible:
                    alpha = 1;
                    break;
                case State.FadeOut:
                    if (_timeSinceLastTransition >= LeaveTime) {
                        return true;
                    }

                    var outProgress = (LeaveTime - _timeSinceLastTransition) / LeaveTime;
                    alpha = outProgress * 1f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Draw(alpha);
            return false;
        }

        public void HighPrio() {
        }

        public void NotifyEnd() {
            SetState(State.FadeOut);
        }

        public bool ShouldBlockInput() {
            return true;
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private void Draw(float alpha) {
            var rect = new Rect(0, 0, UI.screenWidth, UI.screenHeight);

            Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.1f, 0.11f, alpha));
            if (_showLoading && alpha == 1) {
                var beforeFont = Text.Font;
                var beforeAnchor = Text.Anchor;
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = new Color(1, 1, 1, Mathf.Min(1, Mathf.Max(0, Mathf.Sin(_timeSinceLastTransition) + 0.25f)));
                Widgets.Label(new Rect(UI.screenWidth - 150, UI.screenHeight - 80, 100, 45), "LoadingLongEvent".Translate());
                GUI.color = Color.white;
                Text.Font = beforeFont;
                Text.Anchor = beforeAnchor;
            }
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
        }
    }
}