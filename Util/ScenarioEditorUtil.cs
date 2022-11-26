using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace HumanStoryteller.Util; 
public class ScenarioEditorUtil {
	    public static void AddPart(Scenario scen, ScenPart part) {
		    ((List<ScenPart>) Traverse.Create(scen).Field("parts").GetValue()).Add(part);
	    }
	    
	    public static void RemovePart(Scenario scen, Type type) {
		    var parts = (List<ScenPart>) Traverse.Create(scen).Field("parts").GetValue();
		    parts.FindAll(o => o.GetType() == type).ForEach(a => parts.Remove(a));
	    }

	    public static void RemoveStatPart(Scenario scen, StatDef stat) {
		    var parts = (List<ScenPart>) Traverse.Create(scen).Field("parts").GetValue();
		    parts.FindAll(o => o.GetType() == typeof (ScenPart_StatFactor) && Traverse.Create(o).Field("stat").GetValue() == stat).ForEach(a => parts.Remove(a));
	    }
}