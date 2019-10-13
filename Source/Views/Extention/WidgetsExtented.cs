using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HumanStoryteller.Views.Extention {
    public class WidgetsExtented {
        public static void CheckboxBeforeLabel(Rect rect, string label, ref bool checkOn, bool disabled = false, Texture2D texChecked = null,
            Texture2D texUnchecked = null) {
            var lbl = new Rect(rect);
            lbl.x += 24 + 10;
            Widgets.Label(lbl, label);
            if (!disabled && Widgets.ButtonInvisible(lbl)) {
                checkOn = !checkOn;
                if (checkOn)
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                else
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }

            Widgets.CheckboxLabeled(new Rect(rect.position, new Vector2(24, rect.height)), "", ref checkOn, disabled, texChecked, texUnchecked);
        }
    }
}