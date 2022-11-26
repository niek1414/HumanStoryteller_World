using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay; 
[StaticConstructorOnStartup]
public class RadioMessage : IRadioItem, IArchivable {
    private static readonly Texture2D NarratorIcon = ContentFinder<Texture2D>.Get("narrator-icon");

    private const int FadeInTime = 1;
    private const int VisibleTime = 25;
    private const int FadeOutTime = 1;

    private const int CardSpace = 120;

    private const float MaxOpacity = 0.58f;

    private enum State {
        FadeIn,
        Visible,
        FadeOut
    }

    private float _timeSinceLastTransition;
    private State _currentState = State.FadeIn;

    private string _senderName;
    private Pawn _pawn;
    private string _message;
    private int _startingTick;
    private int _id;
    private bool _isGone;

    public RadioMessage() {}

    public RadioMessage(Pawn pawn, string message) : this() {
        _startingTick = GenTicks.TicksGame;
        _id = Rand.Int;
        _pawn = pawn;
        _message = message;
        _isGone = false;
        Find.TickManager.slower.SignalForceNormalSpeedShort();
    }

    public bool Step(ref float verOffset) {
        float cardAlpha;
        float avatarAlpha;
        float horOffset;

        switch (_currentState) {
            case State.FadeIn:
                var inProgress = (FadeInTime - _timeSinceLastTransition) / FadeInTime;
                cardAlpha = (1 - inProgress) * MaxOpacity;
                avatarAlpha = (1 - inProgress) * 1f;
                horOffset = 0;
                Draw(horOffset, verOffset, cardAlpha, avatarAlpha);
                verOffset += (1 - inProgress) * CardSpace;

                _timeSinceLastTransition += Time.deltaTime;
                if (_timeSinceLastTransition >= FadeInTime) {
                    SetState(State.Visible);
                }

                return false;
            case State.Visible:
                if (!Current.Game.tickManager.Paused) {
                    _timeSinceLastTransition += Time.deltaTime;
                }

                if (_timeSinceLastTransition >= VisibleTime) {
                    SetState(State.FadeOut);
                }

                cardAlpha = MaxOpacity;
                avatarAlpha = 1f;
                horOffset = 0;
                break;
            case State.FadeOut:
                _timeSinceLastTransition += Time.deltaTime;
                if (_timeSinceLastTransition >= FadeOutTime) {
                    _isGone = true;
                    Find.History.archive.Add(this);
                    return true;
                }

                var outProgress = (FadeOutTime - _timeSinceLastTransition) / FadeOutTime;
                cardAlpha = outProgress * MaxOpacity;
                avatarAlpha = outProgress * 1f;
                horOffset = (1 - outProgress * outProgress) * 200;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Draw(horOffset, verOffset, cardAlpha, avatarAlpha);
        verOffset += CardSpace;
        return false;
    }

    private void SetState(State state) {
        _currentState = state;
        _timeSinceLastTransition = 0;
    }

    private PortretColor GetColorFromPawn(Pawn pawn, float alpha) {
        PortretColor result;

        if (pawn == null) {
            result = new PortretColor(new Color(0.34f, 0.34f, 0.34f), new Color(0.64f, 0.64f, 0.64f));
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

    private void Draw(float horOffset, float verOffset, float alpha, float masterAlpha) {
        const int textBoxSize = 280;
        var outerRect = new Rect(UI.screenWidth - (textBoxSize + 10) + horOffset, verOffset + 10, textBoxSize, 100);
        Widgets.DrawBoxSolid(outerRect, Mouse.IsOver(outerRect) ? new Color(0.03f, 0.05f, 0.06f, alpha) : new Color(0.08f, 0.1f, 0.11f, alpha));

        Rect portret = new Rect(outerRect) {width = 65, height = 65};
        portret.x += 10;
        portret.y += 10;
        PortretColor pc = GetColorFromPawn(_pawn, masterAlpha);
        Widgets.DrawBoxSolid(portret, pc.Background);
        GUI.color = pc.Edge;
        Widgets.DrawBox(portret);
        var innerPort = portret.ContractedBy(1f);
        Texture icon;
        if (_pawn != null) {
            bool prevPref = Prefs.HatsOnlyOnMap;
            Prefs.HatsOnlyOnMap = false;
            GUI.color = _pawn.relations.everSeenByPlayer ? new Color(1f, 1f, 1f, masterAlpha) : new Color(0f, 0f, 0f, masterAlpha);
            icon = PortraitsCache.Get(_pawn, innerPort.size, Rot4.South, new Vector3(0, 0, 0.4f), 2.5f);
            Prefs.HatsOnlyOnMap = prevPref;
        } else {
            GUI.color = new Color(1f, 1f, 1f, masterAlpha);
            icon = NarratorIcon;
        }

        GUI.DrawTexture(innerPort, icon);
        GUI.color = new Color(1f, 1f, 1f, masterAlpha);
        if (_senderName.NullOrEmpty()) {
            if (_pawn != null) {
                _senderName = _pawn.Name.ToStringShort;
                if (_senderName.Length > 6) {
                    _senderName = _senderName.Substring(0, 5) + ".";
                }
            } else {
                _senderName = "Narrator";
            }
        }

        Rect nameBox = new Rect(outerRect.xMin, portret.yMax + 1, portret.width + 20, 24);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(nameBox, _senderName);

        Rect textBox = new Rect(portret.xMax + 10, portret.yMin, outerRect.width - (portret.width + 30), outerRect.height - 10);
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.Label(textBox, _message);

        if (Widgets.ButtonInvisible(outerRect) && _currentState == State.Visible) {
            SetState(State.FadeOut);
        }

        GUI.color = new Color(1f, 1f, 1f);
    }

    public void ExposeData() {
        Scribe_Values.Look(ref _timeSinceLastTransition, "timeSinceLastTransition");
        Scribe_Values.Look(ref _currentState, "currentState");
        Scribe_References.Look(ref _pawn, "pawn");
        Scribe_Values.Look(ref _message, "description");
        Scribe_Values.Look(ref _senderName, "senderName");
        Scribe_Values.Look(ref _startingTick, "startingTick");
        Scribe_Values.Look(ref _isGone, "isGone");
        Scribe_Values.Look(ref _id, "id");
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

    public string GetUniqueLoadID() {
        return "RADIO_MESSAGE_" + _id;
    }

    public void OpenArchived() {
    }

    public Texture ArchivedIcon {
        get {
            Texture icon;
            if (_pawn != null) {
                bool prevPref = Prefs.HatsOnlyOnMap;
                Prefs.HatsOnlyOnMap = false;
                GUI.color = _pawn.relations.everSeenByPlayer ? new Color(1f, 1f, 1f, 1f) : new Color(0f, 0f, 0f, 1f);
                icon = PortraitsCache.Get(_pawn, new Rect {width = 65, height = 65}.ContractedBy(1f).size, Rot4.South, new Vector3(0, 0, 0.4f), 2.5f);
                GUI.color = new Color(1f, 1f, 1f, 1f);
                Prefs.HatsOnlyOnMap = prevPref;
            } else {
                icon = NarratorIcon;
            }

            return icon;
        }
    }

    public Color ArchivedIconColor => Color.white;

    public string ArchivedLabel => $"{"RadioMessageLabel".Translate()} {_senderName}";

    public string ArchivedTooltip => _message;

    public int CreatedTicksGame => _startingTick;

    public bool CanCullArchivedNow => _isGone;

    public LookTargets LookTargets => _pawn;
}