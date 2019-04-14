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
                faction.TryAffectGoodwillWith(Faction.OfPlayer, Mathf.RoundToInt(allParams.FactionRelation.GetValue()), false, true, null, null);
            } catch (InvalidOperationException) {
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_SetRelation : HumanIncidentParms {
        public Number FactionRelation;
        public string Faction;

        public HumanIncidentParams_SetRelation() {
        }

        public HumanIncidentParams_SetRelation(String target, HumanLetter letter, string faction = "") :
            this(target, letter, new Number(0), faction) {
        }

        public HumanIncidentParams_SetRelation(string target, HumanLetter letter, Number factionRelation, string faction) : base(target, letter) {
            FactionRelation = factionRelation;
            Faction = faction;
        }

        public override string ToString() {
            return $"{base.ToString()}, FactionRelation: {FactionRelation}, Faction: {Faction}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref FactionRelation, "factionRelation");
        }
    }
}