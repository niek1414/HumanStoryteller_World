using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class RadioMessage : IRadioItem {
        private const int VisibleTime = 25;
        private const int FadeOutTime = 1;

        private const float MaxOpacity = 0.58f;

        private enum State {
            Visible,
            FadeOut
        }

        private float _timeSinceLastTransition;
        private State _currentState = State.Visible;

        private Pawn _pawn;
        private string _message;

        public RadioMessage() {
        }

        public RadioMessage(Pawn pawn, string message) {
            _pawn = pawn;
            _message = message;
            Find.TickManager.slower.SignalForceNormalSpeedShort();
        }

        public bool Step(ref float verOffset) {
            float alpha;
            float masterAlpha;
            float horOffset;

            switch (_currentState) {
                case State.Visible:
                    if (!Current.Game.tickManager.Paused) {
                        _timeSinceLastTransition += Time.deltaTime;
                    }
                    if (_timeSinceLastTransition >= VisibleTime) {
                        SetState(State.FadeOut);
                    }

                    alpha = MaxOpacity;
                    masterAlpha = 1f;
                    horOffset = 0;
                    break;
                case State.FadeOut:
                    _timeSinceLastTransition += Time.deltaTime;
                    if (_timeSinceLastTransition >= FadeOutTime) {
                        return true;
                    }

                    var outProgress = (FadeOutTime - _timeSinceLastTransition) / FadeOutTime;
                    alpha = outProgress * MaxOpacity;
                    masterAlpha = outProgress * 1f;
                    horOffset = (1 - outProgress * outProgress) * 200;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            verOffset += Draw(horOffset, verOffset, alpha, masterAlpha);
            return false;
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private PortretColor GetColorFromPawn(Pawn pawn, float alpha) {
            PortretColor result;

            if (pawn == null) {
                result = new PortretColor(new Color(0.18f, 0.18f, 0.18f), new Color(0.3f, 0.3f, 0.3f));
            } else if (pawn.IsPrisoner) {
                result = new PortretColor(new Color(0.46f, 0.45f, 0.24f), new Color(1f, 0.78f, 0f));
            } else if (pawn.IsColonist) {
                result = new PortretColor(new Color(0.24f, 0.38f, 0.47f), new Color(0f, 1f, 1f));
            } else if (pawn.NonHumanlikeOrWildMan()) {
                result = new PortretColor(new Color(0.47f, 0.23f, 0.45f), new Color(0.78f, 0f, 1f));
            } else {
                var relation = pawn.Faction?.RelationWith(Faction.OfPlayer, true)?.kind ?? FactionRelationKind.Neutral;

                if (relation == FactionRelationKind.Ally) {
                    result = new PortretColor(new Color(0.25f, 0.47f, 0.25f), new Color(0.11f, 1f, 0f));
                } else if (relation == FactionRelationKind.Hostile) {
                    result = new PortretColor(new Color(0.47f, 0.22f, 0.23f), new Color(1f, 0f, 0.01f));
                } else {
                    result = new PortretColor(new Color(0.47f, 0.45f, 0.24f), new Color(1f, 0.96f, 0f));
                }
            }

            result.SetAlpha(alpha);
            return result;
        }

        private float Draw(float horOffset, float verOffset, float alpha, float masterAlpha) {
            var outerRect = new Rect(UI.screenWidth - 260 + horOffset, verOffset + 10, 250, 100);
            Widgets.DrawBoxSolid(outerRect, Mouse.IsOver(outerRect) ? new Color(0.03f, 0.05f, 0.06f, alpha) : new Color(0.08f, 0.1f, 0.11f, alpha));

            Rect portret = new Rect(outerRect) {width = 65, height = 65};
            portret.x += 10;
            portret.y += 10;
            PortretColor pc = GetColorFromPawn(_pawn, masterAlpha);
            Widgets.DrawBoxSolid(portret, pc.Background);
            GUI.color = pc.Edge;
            Widgets.DrawBox(portret, 1);
            GUI.color = new Color(1f, 1f, 1f, masterAlpha);
            //TODO by null pawn should use ? or siluete texture
            string n;
            if (_pawn != null) {
                var innerPort = portret.ContractedBy(1f);
                bool prevPref = Prefs.HatsOnlyOnMap;
                Prefs.HatsOnlyOnMap = false;
                GUI.DrawTexture(innerPort, PortraitsCache.Get(_pawn, innerPort.size, new Vector3(0, 0, 0.4f), 2.5f));
                Prefs.HatsOnlyOnMap = prevPref;
                n = _pawn.Name.ToStringShort;
                if (n.Length > 6) {
                    n = n.Substring(0, 5) + ".";
                }
            } else {
                n = "???";
            }

            Rect nameBox = new Rect(outerRect.xMin, portret.yMax + 1, portret.width + 20, 24);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(nameBox, n);

            Rect textBox = new Rect(portret.xMax + 10, portret.yMin, outerRect.width - (portret.width + 30), outerRect.height - 10);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(textBox, _message);

            if (Widgets.ButtonInvisible(outerRect) && _currentState == State.Visible) {
                SetState(State.FadeOut);
            }

            GUI.color = new Color(1f, 1f, 1f);
            return 120;
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
            Scribe_Values.Look(ref _pawn, "title");
            Scribe_Values.Look(ref _message, "description");
        }

        class PortretColor {
            public Color Background;
            public Color Edge;

            public PortretColor(Color background, Color edge) {
                Background = background;
                Edge = edge;
            }

            public void SetAlpha(float alpha) {
                Background.a = alpha;
                Edge.a = alpha;
            }
        }
    }
}