using UnityEngine;

namespace HumanStoryteller.Util.Overlay.BubbleType {
    public interface IBubbleType {
        GUIStyle BubbleBackground();
        Texture2D BubbleEdge();
        Texture2D[] BubbleEdgeArray();
        Texture2D Pointer();

        float MaxOpacity();
        float MaxTextOpacity();
        float MaxLineOpacity();
        void DrawBubbleEdge(Rect rect);
    }
}