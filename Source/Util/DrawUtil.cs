using UnityEngine;
using Verse;

namespace HumanStoryteller.Util {
    public static class DrawUtil {
        public static Rect[] SliceRects(int edge, int size) {
            var zero = 0;
            var one = edge;
            var two = size - edge * 2;
            var three = size - edge;
            // @formatter:off
            return new [] {
                new Rect(zero,   three,   one,     one), 
                new Rect(one,    three,   two,     one), 
                new Rect(three,  three,   one,     one),
                
                new Rect(zero,   one,    one,     two), 
                new Rect(one,    one,    two,     two), 
                new Rect(three,  one,    one,     two), 
                
                new Rect(zero,   zero,   one,     one), 
                new Rect(one,    zero,  two,     one), 
                new Rect(three,  zero,  one,     one) 
            };
            // @formatter:on
        }

        public static GUIStyle Slice9Style(Texture2D texture, RectOffset offset = null) {
            if (offset == null) {
                offset = new RectOffset(20, 20, 20, 20);
            }

            texture.wrapMode = TextureWrapMode.Repeat;
            return new GUIStyle("slice9") {
                normal = {
                    background = texture
                },
                border = offset
            };
        }

        public static Vector2 LabelDrawPosFor(Thing thing, float worldOffsetX, float worldOffsetZ) {
            Vector3 drawPos = thing.DrawPos;
            drawPos.x += worldOffsetX;
            drawPos.z += worldOffsetZ;
            Vector2 vector2 = Find.Camera.WorldToScreenPoint(drawPos) / Prefs.UIScale;
            vector2.y = UI.screenHeight - vector2.y;
            return vector2;
        }

        public static Texture2D GetSlice(Texture2D tex, Rect rect) {
            var x = Mathf.FloorToInt(rect.x);
            var y = Mathf.FloorToInt(rect.y);
            var width = Mathf.FloorToInt(rect.width);
            var height = Mathf.FloorToInt(rect.height);

            Color[] pix;
            try {
                pix = tex.GetPixels(x, y, width, height);
            } catch (UnityException) {
                //Locked
                tex.filterMode = FilterMode.Point;
                RenderTexture rt = RenderTexture.GetTemporary(tex.width, tex.height);
                rt.filterMode = FilterMode.Point;
                RenderTexture.active = rt;
                Graphics.Blit(tex, rt);
                Texture2D img2 = new Texture2D(tex.width, tex.height);
                img2.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                img2.Apply();
                RenderTexture.active = null;
                tex = img2;
                pix = tex.GetPixels(x, y, width, height);
            }

            Texture2D destTex = new Texture2D(width, height);
            destTex.SetPixels(pix);
            destTex.Apply();
            return destTex;
        }

        private static Rect CalcUV(Rect draw, Texture2D tex) {
            return new Rect(0, 0, draw.width / tex.width, draw.height / tex.height);
        }

        public static void DrawAtlas(Rect rect, Texture2D[] slices, float edge) {
            rect.x = Mathf.Round(rect.x);
            rect.y = Mathf.Round(rect.y);
            rect.width = Mathf.Round(rect.width);
            rect.height = Mathf.Round(rect.height);
//            Debug draw all slices
//            for (var i = 0; i < slices.Length; i++) {
//                var slice = slices[i];
//                GUI.DrawTextureWithTexCoords(new Rect(i * 40, 200, slice.width, slice.height), slices[i], new Rect(0, 0, 1, 1));
//            }

            GUI.BeginGroup(rect);
            Rect drawRect;
            Rect uvRect;

            //CORNERS

            //0,0 (0)
            drawRect = new Rect(0.0f, 0.0f, edge, edge);
            uvRect = CalcUV(drawRect, slices[0]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[0]);

            //3,0 (2)
            drawRect = new Rect(rect.width - edge, 0.0f, edge, edge);
            uvRect = CalcUV(drawRect, slices[2]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[2]);

            //0,3 (6)
            drawRect = new Rect(0.0f, rect.height - edge, edge, edge);
            uvRect = CalcUV(drawRect, slices[6]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[6]);

            //3,3 (8)
            drawRect = new Rect(rect.width - edge, rect.height - edge, edge, edge);
            uvRect = CalcUV(drawRect, slices[8]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[8]);

            //CENTER

            //1,1 (4)
            drawRect = new Rect(edge, edge, rect.width - edge * 2f, rect.height - edge * 2f);
            uvRect = CalcUV(drawRect, slices[4]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[4]);

            //EDGES

            //1,0 (1)
            drawRect = new Rect(edge, 0.0f, rect.width - edge * 2f, edge);
            uvRect = CalcUV(drawRect, slices[1]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[1]);

            //1,3 (7)
            drawRect = new Rect(edge, rect.height - edge, rect.width - edge * 2f, edge);
            uvRect = CalcUV(drawRect, slices[7]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[7]);

            //0,1 (3)
            drawRect = new Rect(0.0f, edge, edge, rect.height - edge * 2f);
            uvRect = CalcUV(drawRect, slices[3]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[3]);

            //3,1 (5)
            drawRect = new Rect(rect.width - edge, edge, edge, rect.height - edge * 2f);
            uvRect = CalcUV(drawRect, slices[5]);
            Widgets.DrawTexturePart(drawRect, uvRect, slices[5]);

            GUI.EndGroup();
        }
    }
}