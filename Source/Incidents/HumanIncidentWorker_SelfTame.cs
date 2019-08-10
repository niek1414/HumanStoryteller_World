using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_SelfTame : HumanIncidentWorker {
        public const String Name = "SelfTame";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_SelfTame)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_SelfTame
                allParams = Tell.AssertNotNull((HumanIncidentParams_SelfTame) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            if (!Candidates(map).TryRandomElement(out var pawn))
            {
                return ir;
            }

            pawn.guest?.SetGuestStatus(null);
            string value = pawn.LabelIndefinite();
            PawnUtil.SavePawnByName(allParams.OutName, pawn);
            PawnUtil.SetDisplayName(pawn, allParams.OutName);
            
            bool flag = pawn.Name != null;
            pawn.SetFaction(Faction.OfPlayer);
            string text = flag || pawn.Name == null ? "LetterAnimalSelfTame".Translate(pawn).CapitalizeFirst() : (!pawn.Name.Numerical ? "LetterAnimalSelfTameAndName".Translate(value, pawn.Name.ToStringFull, pawn.Named("ANIMAL")).CapitalizeFirst() : "LetterAnimalSelfTameAndNameNumerical".Translate(value, pawn.Name.ToStringFull, pawn.Named("ANIMAL")).CapitalizeFirst());
            SendLetter(allParams, "LetterLabelAnimalSelfTame".Translate(pawn.KindLabel, pawn).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, pawn);

            return ir;
        }

        private IEnumerable<Pawn> Candidates(Map map)
        {
            return from x in map.mapPawns.AllPawnsSpawned
                where x.RaceProps.Animal && x.Faction == null && !x.Position.Fogged(x.Map) && !x.InMentalState && !x.Downed && x.RaceProps.wildness > 0f
                select x;
        }
    }

    public class HumanIncidentParams_SelfTame : HumanIncidentParms {
        public string OutName = "";

        public HumanIncidentParams_SelfTame() {
        }

        public HumanIncidentParams_SelfTame(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: {OutName}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref OutName, "name");
        }
    }
}