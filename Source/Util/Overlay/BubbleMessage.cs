using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util.Logging;
using HumanStoryteller.Util.Overlay.BubbleType;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay {
    public class BubbleMessage : IBubbleItem, IArchivable {
        private const float FadeInTime = 0.5f;
        private float _visibleTime = 9;
        private const float FadeOutTime = 0.5f;

        private const int FontSize = 17;
        private const int Padding = 11;
        private const int MinBubbleWidth = 250;

        private enum State {
            FadeIn,
            Visible,
            FadeOut
        }

        public enum BubbleType {
            Normal,
            Think,
            Shout,
            Whisper
        }

        private float _oldTime = Time.time;
        private float _timeSinceLastTransition;
        private State _currentState = State.FadeIn;

        private Pawn _pawn;
        private String _message;
        private int _startingTick;
        private int _id;
        private bool _isGone;
        private bool _seen;
        private BubbleType _type;
        private IBubbleType _typeImpl;

        public BubbleMessage() {
        }

        public BubbleMessage(Pawn pawn, String message, BubbleType type) : this() {
            _startingTick = GenTicks.TicksGame;
            _id = Rand.Int;
            _pawn = pawn;
            _isGone = false;
            _message = message;
            _type = type;

            //Adding time to read larger messages
            const float wordsPerSecond = 4f;
            var messageWords = message.Split(' ').Length;
            if (messageWords > 7) {
                _visibleTime += (messageWords - 7) / wordsPerSecond;
            }

            if (_pawn == null) {
                Hide(true);
            }
        }

        private IBubbleType InitType() {
            switch (_type) {
                case BubbleType.Normal:
                    return new NormalBubble();
                case BubbleType.Think:
                    return new ThinkBubble();
                case BubbleType.Shout:
                    return new ShoutBubble();
                case BubbleType.Whisper:
                    return new WhisperBubble();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Hide(bool instant = false) {
            if (instant) {
                SetState(State.FadeOut);
                _timeSinceLastTransition = 500 * 1000;
            }

            if (_seen) {
                _isGone = true;
                Find.History.archive.Add(this);
            }
        }

        public bool Step() {
            if (_typeImpl == null) {
                _typeImpl = InitType();
            }

            var time = Time.realtimeSinceStartup;
            var delta = time - _oldTime;
            _oldTime = time;

            var visible = false;
            if (_pawn.Destroyed || _pawn.Dead) return true;
            if (_pawn.Map == Find.CurrentMap
                && !_pawn.Map.fogGrid.IsFogged(_pawn.Position)
                && Find.CameraDriver.CurrentViewRect.ExpandedBy(10).Contains(_pawn.Position)) {
                _seen = true;
                visible = true;
            }

            float alpha;

            switch (_currentState) {
                case State.FadeIn:
                    var inProgress = (FadeInTime - _timeSinceLastTransition) / FadeInTime;
                    alpha = 1 - inProgress;

                    _timeSinceLastTransition += delta;
                    if (_timeSinceLastTransition >= FadeInTime) {
                        SetState(State.Visible);
                    }

                    break;
                case State.Visible:
                    if (!Current.Game.tickManager.Paused) {
                        _timeSinceLastTransition += delta;
                    }

                    if (_timeSinceLastTransition >= _visibleTime) {
                        SetState(State.FadeOut);
                    }

                    alpha = 1f;
                    break;
                case State.FadeOut:
                    _timeSinceLastTransition += delta;
                    if (_timeSinceLastTransition >= FadeOutTime) {
                        Hide();
                        return true;
                    }

                    alpha = (FadeOutTime - _timeSinceLastTransition) / FadeOutTime;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (visible) {
                Draw(alpha);
            }

            return false;
        }

        private void SetState(State state) {
            _currentState = state;
            _timeSinceLastTransition = 0;
        }

        private Color GetColorFromPawn(Pawn pawn, float alpha) {
            Color color;
            if (pawn.IsPrisoner) {
                color = new Color(0.46f, 0.45f, 0.24f);
            } else if (pawn.IsColonist) {
                color = new Color(0.24f, 0.38f, 0.47f);
            } else if (pawn.NonHumanlikeOrWildMan()) {
                color = new Color(0.47f, 0.23f, 0.45f);
            } else {
                var relation = pawn.Faction?.RelationWith(Faction.OfPlayer, true)?.kind ?? FactionRelationKind.Neutral;
                switch (relation) {
                    case FactionRelationKind.Ally:
                        color = new Color(0.25f, 0.47f, 0.25f);
                        break;
                    case FactionRelationKind.Hostile:
                        color = new Color(0.47f, 0.22f, 0.23f);
                        break;
                    default:
                        color = new Color(0.47f, 0.45f, 0.24f);
                        break;
                }
            }

            color.a = alpha;
            if (_type == BubbleType.Shout) {
                color.SaturationChanged(0.1f);
            }

            return color;
        }

        private void Draw(float alpha) {
            var originalMatrix = GUI.matrix;
            try {
                var font = new GUIStyle(Text.fontStyles[(int) GameFont.Medium]) {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Overflow,
                    padding = new RectOffset(0, 0, 0, 0),
                    fontSize = FontSize
                };
                var paddingHeight = Mathf.CeilToInt(Padding);
                var paddingWidth = paddingHeight;
                var content = new GUIContent(_message);

                var contentSize = font.CalcSize(content);
                var width = Mathf.CeilToInt(contentSize.x + paddingWidth * 2);
                if (width < MinBubbleWidth) {
                    paddingWidth += Mathf.RoundToInt((MinBubbleWidth - width) / 2f);
                    width = Mathf.CeilToInt(contentSize.x + paddingWidth * 2);
                }

                var height = Mathf.CeilToInt(contentSize.y + paddingHeight * 2);
                var pos = DrawUtil.LabelDrawPosFor(_pawn, 0.3f, 1.6f);

                var outer = new Rect(pos.x - width / 2f, pos.y - height, width, height);
                var inner = new Rect(outer.x + paddingWidth, outer.y + paddingHeight, outer.width - paddingWidth * 2f,
                    outer.height - paddingHeight * 2f);

                var pointer = _typeImpl.Pointer();

                //Scale
                var scale = CalcScale();
                var pivotPoint = DrawUtil.LabelDrawPosFor(_pawn, 0f, 0f);
                pivotPoint.y -= (height + pointer.height) / 2f;
                GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), pivotPoint);

                alpha *= Mathf.Min((scale - .4f) / (.55f - .4f), 1);

                var textAlpha = alpha * _typeImpl.MaxTextOpacity();
                var lineAlpha = alpha * _typeImpl.MaxLineOpacity();
                var backgroundAlpha = alpha * _typeImpl.MaxOpacity();

                GUI.color = GetColorFromPawn(_pawn, alpha);
                GUI.DrawTexture(new Rect(pos.x, pos.y, pointer.width, pointer.height), pointer);

                GUI.color = new Color(0.08f, 0.1f, 0.11f, backgroundAlpha);
                GUI.Label(outer, "", _typeImpl.BubbleBackground());

                GUI.color = GetColorFromPawn(_pawn, lineAlpha);
                _typeImpl.DrawBubbleEdge(outer);

                GUI.color = new Color(1f, 1f, 1f, textAlpha);
                GUI.Label(inner, content, font);
            } finally {
                GUI.matrix = originalMatrix;
                GUI.color = Color.white;
            }
        }

        public void OtherBubbleAdded(Pawn p) {
            if (_pawn.Equals(p)) {
                Hide(true);
            }
        }

        public Pawn GetOwner() {
            return _pawn;
        }

        private float CalcScale() {
            const float optimal = 45f;
            const float minZoom = 8f;
            const float range = optimal - minZoom;
            return Mathf.Min(Find.CameraDriver.CellSizePixels / range, 1f);
        }

        public void ExposeData() {
            Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
            Scribe_Values.Look(ref _currentState, "currentState");
            Scribe_References.Look(ref _pawn, "pawn");
            Scribe_Deep.Look(ref _message, "message");
            Scribe_Values.Look(ref _startingTick, "startingTick");
            Scribe_Values.Look(ref _type, "type");
            Scribe_Values.Look(ref _isGone, "isGone");
            Scribe_Values.Look(ref _id, "id");
            Scribe_Values.Look(ref _visibleTime, "visibleTime");
        }

        public string GetUniqueLoadID() {
            return "BUBBLE_MESSAGE_" + _id;
        }

        public void OpenArchived() {
        }

        public Texture ArchivedIcon {
            get {
                Texture icon = null;
                if (_pawn != null) {
                    bool prevPref = Prefs.HatsOnlyOnMap;
                    Prefs.HatsOnlyOnMap = false;
                    GUI.color = _pawn.relations.everSeenByPlayer ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                    icon = PortraitsCache.Get(_pawn, new Rect {width = 65, height = 65}.ContractedBy(1f).size, new Vector3(0, 0, 0.4f),
                        2.5f);
                    GUI.color = new Color(1f, 1f, 1f, 1f);
                    Prefs.HatsOnlyOnMap = prevPref;
                }

                return icon;
            }
        }

        public Color ArchivedIconColor => Color.white;

        public string ArchivedLabel => _message;

        public string ArchivedTooltip => _message;

        public int CreatedTicksGame => _startingTick;

        public bool CanCullArchivedNow => _isGone;

        public LookTargets LookTargets => _pawn;
    }
}