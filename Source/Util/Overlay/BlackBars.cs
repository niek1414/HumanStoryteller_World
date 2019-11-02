using System;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class BlackBars : IOverlayItem {
        private const int EnterTime = 3;
        private const int LeaveTime = 3;

        public const float MaxCoverPercent = 0.1f;
        public float LastBarHeight;

        private enum State {
            Enter,
            Visible,
            Leave
        }

        private float _timeSinceLastTransition;
        private State _currentState = State.Enter;

        public BlackBars() {
        }


        public bool Step() {
            _timeSinceLastTransition += Time.deltaTime;
            float coverSize;

            switch (_currentState) {
                case State.Enter:
                    if (_timeSinceLastTransition >= EnterTime) {
                        SetState(State.Visible);
                        coverSize = MaxCoverPercent;
                    } else {
                        var progress = (EnterTime - _timeSinceLastTransition) / EnterTime;
                        coverSize = (1 - progress) * MaxCoverPercent;
                    }

                    break;
                case State.Visible:
                    if (!HumanStoryteller.StoryComponent.StoryStatus.MovieMode) {
                        SetState(State.Leave);
                    }

                    coverSize = MaxCoverPercent;
                    break;
                case State.Leave:
                    if (_timeSinceLastTransition >= LeaveTime) {
                        return true;
                    }

                    var outProgress = (LeaveTime - _timeSinceLastTransition) / LeaveTime;
                    coverSize = outProgress * MaxCoverPercent;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LastBarHeight = coverSize;
            Draw(coverSize);
            return false;
        }

        public void HighPrio() {
        }

        public void NotifyEnd() {
            SetState(State.Leave);
        }

        public bool ShouldBlockInput() {
            return false;
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private void Draw(float coverSize) {
            var barHeight = coverSize * UI.screenHeight;
            var topRect = new Rect(0, 0, UI.screenWidth, barHeight);
            var bottomRect = new Rect(0, UI.screenHeight - barHeight, UI.screenWidth, barHeight);

            Widgets.DrawBoxSolid(topRect, new Color(0.08f, 0.1f, 0.11f));
            Widgets.DrawBoxSolid(bottomRect, new Color(0.08f, 0.1f, 0.11f));
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
        }
    }
}