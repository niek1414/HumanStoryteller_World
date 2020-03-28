using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util.Logging;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Disease : HumanIncidentWorker {
        public const String Name = "Disease";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_Disease)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Disease allParams =
                Tell.AssertNotNull((HumanIncidentParams_Disease) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            IncidentDef def = null;
            if (allParams.Disease != "") {
                try {
                    def = (from x in DefDatabase<IncidentDef>.AllDefsListForReading
                        where x.defName == allParams.Disease
                        select x).First();
                } catch (InvalidOperationException) {
                }
            }

            if (def == null) {
                if (!(from x in DefDatabase<IncidentDef>.AllDefsListForReading
                    where x.category == (Rand.Bool ? IncidentCategoryDefOf.DiseaseHuman : IncidentCategoryDefOf.DiseaseAnimal)
                    select x).TryRandomElementByWeight(d => map.Biome.CommonalityOfDisease(d), out IncidentDef def2)) {
                    return ir;
                }

                def = def2;
            }

            List<Pawn> list = new List<Pawn>();
            foreach (var target in allParams.Pawns.Filter(map)) {
                if (target.DestroyedOrNull() || target.Dead) continue;
                list.Add(target);
            }

            if (list.Count == 0) {
                list.AddRange(ActualVictims(def, map));
            }

            string blockedInfo;
            list = ApplyToPawns(def, list, out blockedInfo);
            if (!list.Any() && blockedInfo.NullOrEmpty()) {
                return ir;
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < list.Count; i++) {
                if (stringBuilder.Length != 0) {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append("  - " + list[i].LabelCap);
            }

            string text = !list.Any()
                ? string.Empty
                : string.Format(def.letterText, list.Count.ToString(), Faction.OfPlayer.def.pawnsPlural, def.diseaseIncident.label,
                    stringBuilder);
            if (!blockedInfo.NullOrEmpty()) {
                if (!text.NullOrEmpty()) {
                    text += "\n\n";
                }

                text += blockedInfo;
            }

            SendLetter(allParams, def.letterLabel, text, def.letterDef, list);
            return ir;
        }

        public List<Pawn> ApplyToPawns(IncidentDef def, IEnumerable<Pawn> pawns, out string blockedInfo) {
            List<Pawn> list = new List<Pawn>();
            Dictionary<HediffDef, List<Pawn>> dictionary = new Dictionary<HediffDef, List<Pawn>>();
            foreach (Pawn pawn in pawns) {
                HediffDef immunityCause;
                if (Rand.Chance(pawn.health.immunity.DiseaseContractChanceFactor(def.diseaseIncident, out immunityCause))) {
                    HediffGiverUtility.TryApply(pawn, def.diseaseIncident, def.diseasePartsToAffect);
                    TaleRecorder.RecordTale(TaleDefOf.IllnessRevealed, pawn, def.diseaseIncident);
                    list.Add(pawn);
                } else if (immunityCause != null) {
                    if (!dictionary.ContainsKey(immunityCause)) {
                        dictionary[immunityCause] = new List<Pawn>();
                    }

                    dictionary[immunityCause].Add(pawn);
                }
            }

            blockedInfo = string.Empty;
            foreach (KeyValuePair<HediffDef, List<Pawn>> item in dictionary) {
                if (item.Key != def.diseaseIncident) {
                    if (blockedInfo.Length != 0) {
                        blockedInfo += "\n\n";
                    }

                    blockedInfo += "LetterDisease_Blocked".Translate(item.Key.LabelCap, def.diseaseIncident.label, (from victim in item.Value
                        select victim.LabelShort).ToCommaList(true));
                }
            }

            return list;
        }

        private IEnumerable<Pawn> PotentialVictimCandidates(IncidentDef def, IIncidentTarget target) {
            if (def.category == IncidentCategoryDefOf.DiseaseAnimal) {
                if (target is Map map) {
                    return from p in map.mapPawns.PawnsInFaction(Faction.OfPlayer)
                        where p.HostFaction == null && !p.RaceProps.Humanlike
                        select p;
                }

                return from p in ((Caravan) target).PawnsListForReading
                    where !p.RaceProps.Humanlike
                    select p;
            } else {
                if (target is Map map) {
                    return map.mapPawns.FreeColonistsAndPrisoners;
                }

                return from x in ((Caravan) target).PawnsListForReading
                    where x.IsFreeColonist || x.IsPrisonerOfColony
                    select x;
            }
        }

        private IEnumerable<Pawn> ActualVictims(IncidentDef def, IIncidentTarget target) {
            if (def.category == IncidentCategoryDefOf.DiseaseAnimal) {
                Pawn[] potentialVictims = PotentialVictimCandidates(def, target).ToArray();
                IEnumerable<ThingDef> source = (from v in potentialVictims select v.def).Distinct();
                ThingDef targetRace = source.RandomElementByWeightWithFallback(race => (
                    from v in potentialVictims
                    where v.def == race
                    select v.BodySize).Sum());
                IEnumerable<Pawn> source2 = 
                    from v in potentialVictims
                    where v.def == targetRace
                    select v;
                int num = source2.Count();
                int randomInRange = new IntRange(Mathf.RoundToInt(num * def.diseaseVictimFractionRange.min),
                    Mathf.RoundToInt(num * def.diseaseVictimFractionRange.max)).RandomInRange;
                randomInRange = Mathf.Clamp(randomInRange, 1, def.diseaseMaxVictims);
                return source2.InRandomOrder().Take(randomInRange);
            } else {
                int num = PotentialVictimCandidates(def, target).Count();
                int randomInRange = new IntRange(Mathf.RoundToInt(num * def.diseaseVictimFractionRange.min),
                    Mathf.RoundToInt(num * def.diseaseVictimFractionRange.max)).RandomInRange;
                randomInRange = Mathf.Clamp(randomInRange, 1, def.diseaseMaxVictims);
                return PotentialVictimCandidates(def, target).InRandomOrder().Take(randomInRange);
            }
        }
    }

    public class HumanIncidentParams_Disease : HumanIncidentParms {
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string Disease = "";

        public HumanIncidentParams_Disease() {
        }

        public HumanIncidentParams_Disease(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref Disease, "disease");
        }
    }
}