using System;
using System.Linq;
using HumanStoryteller.Incidents.GameConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SetRelation : HumanIncidentWorker {
        public const String Name = "SetRelation";

        public override IncidentResult Execute(HumanIncidentParms parms) {
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
        public Number FactionRelation;
        public string Faction;
        public string NewName;

        public HumanIncidentParams_SetRelation() {
        }

        public HumanIncidentParams_SetRelation(String target, HumanLetter letter, string faction = "", string newName = "") :
            this(target, letter, new Number(0), faction, newName) {
        }

        public HumanIncidentParams_SetRelation(string target, HumanLetter letter, Number factionRelation, string faction, string newName) : base(target, letter) {
            FactionRelation = factionRelation;
            Faction = faction;
            NewName = newName;
        }

        public override string ToString() {
            return $"{base.ToString()}, FactionRelation: {FactionRelation}, Faction: {Faction}, NewName: {NewName}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref FactionRelation, "factionRelation");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref NewName, "newName");
        }
    }
}