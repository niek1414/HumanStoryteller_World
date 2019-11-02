using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_RefugeePodCrash : HumanIncidentWorker {
        public const String Name = "RefugeePodCrash";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_RefugeePodCrash)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_RefugeePodCrash
                allParams = Tell.AssertNotNull((HumanIncidentParams_RefugeePodCrash) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            List<Thing> things = ThingSetMakerDefOf.RefugeePod.root.Generate();
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            Pawn pawn = FindPawn(things);
            
            if (allParams.OutName != "") {
                PawnUtil.SavePawnByName(allParams.OutName, pawn);
            }
            
            PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);
            
            pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = true;
            string title = "LetterLabelRefugeePodCrash".Translate();
            string text = "RefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn);
            text += "\n\n";
            text = pawn.Faction == null
                ? text + "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn)
                : (!pawn.Faction.HostileTo(Faction.OfPlayer)
                    ? text + "RefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn)
                    : text + "RefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn));
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref title, pawn);
            SendLetter(allParams, title, text, LetterDefOf.NeutralEvent, new TargetInfo(intVec, map));
            ActiveDropPodInfo activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAddRangeOrTransfer(things);
            activeDropPodInfo.openDelay = 180;
            activeDropPodInfo.leaveSlag = true;
            DropPodUtility.MakeDropPodAt(intVec, map, activeDropPodInfo);

            return ir;
        }

        private static Pawn FindPawn(IEnumerable<Thing> things) {
            foreach (var t in things) {
                switch (t) {
                    case Pawn pawn:
                        return pawn;
                    case Corpse corpse:
                        return corpse.InnerPawn;
                }
            }

            return null;
        }
    }

    public class HumanIncidentParams_RefugeePodCrash : HumanIncidentParms {
        public string OutName = "";
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";

        public HumanIncidentParams_RefugeePodCrash() {
        }

        public HumanIncidentParams_RefugeePodCrash(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, OutName: [{OutName}], FirstName: [{FirstName}], NickName: [{NickName}], LastName: [{LastName}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref OutName, "name");
            Scribe_Values.Look(ref FirstName, "firstName");
            Scribe_Values.Look(ref NickName, "nickName");
            Scribe_Values.Look(ref LastName, "lastName");
        }
    }
}