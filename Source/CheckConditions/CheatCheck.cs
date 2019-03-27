using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class CheatCheck : CheckCondition {
        public const String Name = "Cheat";

        public CheatCheck() {
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return Prefs.DevMode;
        }
    }
}