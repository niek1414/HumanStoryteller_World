using System;
using HumanStoryteller.Util.Logging;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class ShowImage : IOverlayItem {
        private const int EnterTime = 1;
        private const int LeaveTime = 1;

        private enum State {
            FadeIn,
            Visible,
            FadeOut
        }

        private float _timeSinceLastTransition;
        private State _currentState = State.FadeIn;
        private string _url;
        private Texture2D _loadedImage = null;
        private WWW _loadedingImage = null;

        public ShowImage() {
        }

        public ShowImage(string url) {
            _url = url;
        }


        public bool Step() {
            if (_loadedImage == null) {
                if (_loadedingImage == null) {
                    _loadedingImage = new WWW(GenFilePaths.SafeURIForUnityWWWFromPath(_url).Substring(8));
                }

                try {
                    if (_loadedingImage.isDone) {
                        if (_loadedingImage.error == null) {
                            _loadedImage = _loadedingImage.textureNonReadable;
                        } else {
                            Tell.Warn("Could not load image", "url: " + _url, "Err: " + _loadedingImage.error);
                            SetState(State.FadeOut);
                        }

                        _loadedingImage?.Dispose();
                    }
                } catch (Exception e) {
                    try {
                        Tell.Warn("Could not load image ", "url: " + _url, "Err: " + e.Message + " Stack__ " + e.StackTrace);
                        _loadedingImage?.Dispose();
                        SetState(State.FadeOut);
                    } catch (Exception) {
                        // ignored
                    }
                }

                return false;
            }

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
            return _loadedImage != null;
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private void Draw(float alpha) {
            if (_loadedImage == null) return;
            var rect = new Rect(0, 0, UI.screenWidth, UI.screenHeight);
            
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(rect, _loadedImage, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
            Scribe_Values.Look(ref _url, "url");
        }
    }
}