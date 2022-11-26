using RimWorld;
using Verse;

namespace HumanStoryteller.Util; 
public class ItemUtil {
    public static QualityCategory GetCategory(string q, QualityCategory fallback) {
        switch (q) {
            case "Awful":
                return QualityCategory.Awful;
            case "Poor":
                return QualityCategory.Poor;
            case "Normal":
                return QualityCategory.Normal;
            case "Good":
                return QualityCategory.Good;
            case "Excellent":
                return QualityCategory.Excellent;
            case "Masterwork":
                return QualityCategory.Masterwork;
            default:
                return fallback;
        }
    }

    public static void TrySetQuality(Thing t, QualityCategory qc) {
        CompQuality compQuality = t is MinifiedThing minifiedThing
            ? minifiedThing.InnerThing.TryGetComp<CompQuality>()
            : t.TryGetComp<CompQuality>();
        compQuality?.SetQuality(qc, ArtGenerationContext.Colony);
    }

    public static Thing TryMakeMinified(Thing t) {
        if (t == null) return null;
        if (t is MinifiedThing minifiedThing && minifiedThing.InnerThing != null) {
            return minifiedThing.InnerThing.TryMakeMinified();
        }

        return t.TryMakeMinified();
    }
}