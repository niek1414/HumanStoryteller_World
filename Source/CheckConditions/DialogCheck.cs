using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class DialogCheck : CheckCondition {
        public const String Name = "Dialog";
        
        private bool _onAccepted;

        public DialogCheck() {
        }

        public DialogCheck(bool onAccepted) {
            _onAccepted = Tell.AssertNotNull(onAccepted, nameof(onAccepted), GetType().Name);
        }

        public override bool Check(StoryNode sn) {
            return false;
        }

        public override string ToString() {
            return $"OnAccepted: {_onAccepted}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref _onAccepted, "onAccepted");
        }
    }
}