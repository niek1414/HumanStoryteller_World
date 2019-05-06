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

            if (!Candidates(map).TryRandomElement(out var result))
            {
                return ir;
            }

            result.guest?.SetGuestStatus(null);
            string value = result.LabelIndefinite();
            if (allParams.Name != "") {
                switch (result.Name) {
                    case NameTriple prevNameTriple:
                        result.Name = new NameTriple(allParams.Name, allParams.Name, prevNameTriple.Last);
                        break;
                    case NameSingle prevNameSingle:
                        result.Name = new NameTriple(allParams.Name, allParams.Name, prevNameSingle.Name);
                        break;
                    default:
                        result.Name = new NameTriple(allParams.Name, allParams.Name, "");
                        break;
                }
            }
            bool flag = result.Name != null;
            result.SetFaction(Faction.OfPlayer);
            string text = flag || result.Name == null ? "LetterAnimalSelfTame".Translate(result).CapitalizeFirst() : (!result.Name.Numerical ? "LetterAnimalSelfTameAndName".Translate(value, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst() : "LetterAnimalSelfTameAndNameNumerical".Translate(value, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst());
            SendLetter(allParams, "LetterLabelAnimalSelfTame".Translate(result.KindLabel, result).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, result);

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
        public string Name;

        public HumanIncidentParams_SelfTame() {
        }

        public HumanIncidentParams_SelfTame(String target, HumanLetter letter, string name = "") : base(target, letter) {
            Name = name;
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: {Name}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Name, "name");
        }
    }
}