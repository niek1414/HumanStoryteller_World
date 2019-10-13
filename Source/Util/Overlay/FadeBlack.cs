using System;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class FadeBlack : IOverlayItem {
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

        public void NotifyEnd() {
            SetState(State.FadeOut);
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private void Draw(float alpha) {
            var rect = new Rect(0, 0, UI.screenWidth, UI.screenHeight);

            Widgets.DrawBoxSolid(rect, new Color(0.08f, 0.1f, 0.11f, alpha));
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
        }
    }
}