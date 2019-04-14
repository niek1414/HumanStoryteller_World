using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RenamePawn : HumanIncidentWorker {
        public const String Name = "RenamePawn";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_RenamePawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RenamePawn
                allParams = Tell.AssertNotNull((HumanIncidentParams_RenamePawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Pawn pawn = null;
            if (allParams.Name != "") {
                pawn = PawnUtil.GetPawnByName(allParams.Name);
            } else {
                if (PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.Where(p => !PawnUtil.PawnExists(p))
                    .TryRandomElement(out Pawn result)) {
                    pawn = result;
                }
            }

            if (pawn == null)
                return ir;

            PawnUtil.RemoveName(allParams.Name);
            PawnUtil.SavePawnByName(allParams.NewName, pawn);

            return ir;
        }
    }

    public class HumanIncidentParams_RenamePawn : HumanIncidentParms {
        public string NewName;
        public string Name;

        public HumanIncidentParams_RenamePawn() {
        }

        public HumanIncidentParams_RenamePawn(String target, HumanLetter letter, string newName = "", string name = "") : base(target, letter) {
            NewName = newName;
            Name = name;
        }

        public override string ToString() {
            return $"{base.ToString()}, NewName: {NewName}, Name: {Name}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref NewName, "newName");
            Scribe_Values.Look(ref Name, "name");
        }
    }
}