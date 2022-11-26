using System.Collections.Generic;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.Helper.QuestHelper; 
public class QuestSitePart : SitePart {
    private List<string> _names;

    public QuestSitePart() {
    }

    public QuestSitePart(List<string> names) {
        _names = names;
        parms = null;
        def = new QuestSitePartDef(_names);
    }

    public override string ToString() {
        return $"Names: [{_names.Join()}]";
    }
    
    public new void ExposeData() {
        base.ExposeData();
        Scribe_Collections.Look(ref _names, "names", LookMode.Value);
        if (Scribe.mode == LoadSaveMode.LoadingVars) {
            def = new QuestSitePartDef(_names);
        }
    }
}