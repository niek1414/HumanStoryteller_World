using UnityEngine;
using Verse;

namespace HumanStoryteller.Util.Overlay.BubbleType; 
[StaticConstructorOnStartup]
public class WhisperBubble : IBubbleType {
    private static readonly Texture2D StaticBubbleBackground = ContentFinder<Texture2D>.Get("normal-bubble-background");
    private static readonly Texture2D StaticBubbleEdge = ContentFinder<Texture2D>.Get("whisper-bubble-edge");
    private static readonly Texture2D StaticPointer = ContentFinder<Texture2D>.Get("normal-pointer");

    private const int StaticEdgeSlice = 4;
    private const int StaticEdgeFull = 32;
    private static readonly Texture2D[] StaticBubbleEdgeSliced;

    private const float StaticMaxOpacity = 0.45f;
    private const float StaticMaxLineOpacity = 0.55f;
    private const float StaticMaxTextOpacity = 0.60f;

    static WhisperBubble() {
        StaticBubbleEdge.wrapMode = TextureWrapMode.Repeat;
        StaticBubbleEdgeSliced = new Texture2D[9];
        var slice = DrawUtil.SliceRects(StaticEdgeSlice, StaticEdgeFull);
        for (var i = 0; i < slice.Length; i++) {
            StaticBubbleEdgeSliced[i] = DrawUtil.GetSlice(StaticBubbleEdge, slice[i]);
            StaticBubbleEdgeSliced[i].wrapMode = TextureWrapMode.Repeat;
        }
    }
    
    public GUIStyle BubbleBackground() {
        return DrawUtil.Slice9Style(StaticBubbleBackground);
    }

    public Texture2D BubbleEdge() {
        StaticBubbleEdge.wrapMode = TextureWrapMode.Repeat;
        return StaticBubbleEdge;
    }

    public Texture2D[] BubbleEdgeArray() {
        return StaticBubbleEdgeSliced;
    }

    public Texture2D Pointer() {
        return StaticPointer;
    }

    public float MaxOpacity() {
        return StaticMaxOpacity;
    }

    public float MaxTextOpacity() {
        return StaticMaxTextOpacity;
    }

    public float MaxLineOpacity() {
        return StaticMaxLineOpacity;
    }

    public void DrawBubbleEdge(Rect rect) {
        DrawUtil.DrawAtlas(rect, StaticBubbleEdgeSliced, StaticEdgeSlice);
    }
}