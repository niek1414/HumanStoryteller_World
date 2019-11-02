using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RenamePawn : HumanIncidentWorker {
        public const String Name = "RenamePawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_RenamePawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RenamePawn
                allParams = Tell.AssertNotNull((HumanIncidentParams_RenamePawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            Pawn pawn = null;
            if (allParams.UnnamedColonist) {
                if (map.mapPawns.FreeColonists.Where(p => !PawnUtil.PawnExists(p)).TryRandomElement(out Pawn result)) {
                    pawn = result;
                }
            } else {
                pawn = allParams.Pawns.Filter(map).RandomElementWithFallback();
            }

            if (pawn == null)
                return ir;

            PawnUtil.RemovePawn(pawn);
            PawnUtil.SavePawnByName(allParams.OutName, pawn);

            SendLetter(allParams);
            return ir;
        }
    }

    public class HumanIncidentParams_RenamePawn : HumanIncidentParms {
        public PawnGroupSelector Pawns;
        public bool UnnamedColonist;
        public string OutName = "";

        public HumanIncidentParams_RenamePawn() {
        }

        public HumanIncidentParams_RenamePawn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, UnnamedColonist: [{UnnamedColonist}], NewName: [{OutName}], Pawns: [{Pawns}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref UnnamedColonist, "unnamedColonist");
            Scribe_Values.Look(ref OutName, "newName");
            Scribe_Deep.Look(ref Pawns, "pawns");
        }
    }
}