using System;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SetRelation : HumanIncidentWorker {
        public const String Name = "SetRelation";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_SetRelation)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_SetRelation allParams = Tell.AssertNotNull((HumanIncidentParams_SetRelation) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");


            Faction faction;
            try {
                faction = Find.FactionManager.AllFactions.First(f => f.def.defName == allParams.Faction);
                var relationFlux = allParams.FactionRelation.GetValue();
                if (relationFlux != 0) {
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, Mathf.RoundToInt(relationFlux), false, true, null, null);
                }

                if (allParams.NewName != "") {
                    faction.Name = allParams.NewName;
                }
            } catch (InvalidOperationException) {
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_SetRelation : HumanIncidentParms {
        public Number FactionRelation = new Number(0);
        public string Faction = "";
        public string NewName = "";

        public HumanIncidentParams_SetRelation() {
        }

        public HumanIncidentParams_SetRelation(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, FactionRelation: [{FactionRelation}], Faction: [{Faction}], NewName: [{NewName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref FactionRelation, "factionRelation");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref NewName, "newName");
        }
    }
}