using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_EditPawn : HumanIncidentWorker {
        public const String Name = "EditPawn";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_EditPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_EditPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_EditPawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            foreach (var name in allParams.Names) {
                var pawn = PawnUtil.GetPawnByName(name);
                if (pawn == null) {
                    continue;
                }

                if (allParams.Strip) {
                    pawn.Strip();
                }

                if (allParams.ClearMind) {
                    pawn.ClearMind();
                }

                if (allParams.Faction != "") {
                    pawn.SetFaction(FactionUtility.DefaultFactionFrom(FactionDef.Named(allParams.Faction)));
                }

                if (allParams.Location != "") {
                    IntVec3 intVec;
                    switch (allParams.Location) {
                        case "RandomEdge":
                            intVec = CellFinder.RandomEdgeCell(map);
                            break;
                        case "Center":
                            intVec = RCellFinder.TryFindRandomCellNearWith(map.Center, null, map, out var result)
                                ? result
                                : DropCellFinder.RandomDropSpot(map);
                            break;
                        default:
                            intVec = DropCellFinder.RandomDropSpot(map);
                            break;
                    }

                    pawn.SetPositionDirect(intVec);
                }

                if (allParams.Banish) {
                    PawnBanishUtility.Banish(pawn);
                }

                if (allParams.SetDrafted) {
                    pawn.drafter.Drafted = true;
                }
 
                if (allParams.AgeBioYear != -1){
                    pawn.ageTracker.AgeBiologicalTicks = Mathf.RoundToInt(allParams.AgeBioYear * 3600000L);
                }

                if (allParams.SkillAnimals != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Animals).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Animals).Level : 0) + allParams.SkillAnimals);
                }

                if (allParams.SkillArtistic != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Artistic).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Artistic).Level : 0) + allParams.SkillArtistic);
                }

                if (allParams.SkillConstruction != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Construction).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Construction).Level : 0) +
                                         allParams.SkillConstruction);
                }

                if (allParams.SkillCooking != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Cooking).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Cooking).Level : 0) + allParams.SkillCooking);
                }

                if (allParams.SkillCrafting != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Crafting).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Crafting).Level : 0) + allParams.SkillCrafting);
                }

                if (allParams.SkillPlants != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Plants).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Plants).Level : 0) + allParams.SkillPlants);
                }

                if (allParams.SkillMedicine != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Medicine).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Medicine).Level : 0) + allParams.SkillMedicine);
                }

                if (allParams.SkillMelee != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Melee).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Melee).Level : 0) + allParams.SkillMelee);
                }

                if (allParams.SkillMining != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Mining).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Mining).Level : 0) + allParams.SkillMining);
                }

                if (allParams.SkillIntellectual != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Intellectual).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Intellectual).Level : 0) +
                                         allParams.SkillIntellectual);
                }

                if (allParams.SkillShooting != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Shooting).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Shooting).Level : 0) + allParams.SkillShooting);
                }

                if (allParams.SkillSocial != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Social).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Social).Level : 0) + allParams.SkillSocial);
                }
            }

            if (parms.Letter?.Type != null) {
                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_EditPawn : HumanIncidentParms {
        public List<String> Names;
        public bool Strip;
        public bool ClearMind;
        public bool Banish;
        public bool SetDrafted;
        public float AgeBioYear;
        public string Faction;
        public string Location;

        public bool SkillAdd;
        public float SkillAnimals;
        public float SkillArtistic;
        public float SkillConstruction;
        public float SkillCooking;
        public float SkillCrafting;
        public float SkillPlants;
        public float SkillMedicine;
        public float SkillMelee;
        public float SkillMining;
        public float SkillIntellectual;
        public float SkillShooting;
        public float SkillSocial;

        public HumanIncidentParams_EditPawn() {
        }

        public HumanIncidentParams_EditPawn(String target, HumanLetter letter,
            List<String> names = null,
            bool strip = false,
            bool clearMind = false,
            bool banish = false,
            bool setDrafted = false,
            float ageBioYear = -1,
            string faction = "",
            string location = "",
            bool skillAdd = false,
            float skillAnimals = -1,
            float skillArtistic = -1,
            float skillConstruction = -1,
            float skillCooking = -1,
            float skillCrafting = -1,
            float skillPlants = -1,
            float skillMedicine = -1,
            float skillMelee = -1,
            float skillMining = -1,
            float skillIntellectual = -1,
            float skillShooting = -1,
            float skillSocial = -1
        ) :
            base(target, letter) {
            Names = names ?? new List<string>();
            Strip = strip;
            ClearMind = clearMind;
            Banish = banish;
            SetDrafted = setDrafted;
            AgeBioYear = ageBioYear;
            Faction = faction;
            Location = location;

            SkillAdd = skillAdd;
            SkillAnimals = skillAnimals;
            SkillArtistic = skillArtistic;
            SkillConstruction = skillConstruction;
            SkillCooking = skillCooking;
            SkillCrafting = skillCrafting;
            SkillPlants = skillPlants;
            SkillMedicine = skillMedicine;
            SkillMelee = skillMelee;
            SkillMining = skillMining;
            SkillIntellectual = skillIntellectual;
            SkillShooting = skillShooting;
            SkillSocial = skillSocial;
        }


        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref Strip, "strip");
            Scribe_Values.Look(ref ClearMind, "clearMind");
            Scribe_Values.Look(ref Banish, "banish");
            Scribe_Values.Look(ref SetDrafted, "setDrafted");
            Scribe_Values.Look(ref AgeBioYear, "ageBioYear");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref Location, "location");

            Scribe_Values.Look(ref SkillAdd, "skillAdd");
            Scribe_Values.Look(ref SkillAnimals, "skillAnimals");
            Scribe_Values.Look(ref SkillArtistic, "skillArtistic");
            Scribe_Values.Look(ref SkillConstruction, "skillConstruction");
            Scribe_Values.Look(ref SkillCooking, "skillCooking");
            Scribe_Values.Look(ref SkillCrafting, "skillCrafting");
            Scribe_Values.Look(ref SkillPlants, "skillPlants");
            Scribe_Values.Look(ref SkillMedicine, "skillMedicine");
            Scribe_Values.Look(ref SkillMelee, "skillMelee");
            Scribe_Values.Look(ref SkillMining, "skillMining");
            Scribe_Values.Look(ref SkillIntellectual, "skillIntellectual");
            Scribe_Values.Look(ref SkillShooting, "skillShooting");
            Scribe_Values.Look(ref SkillSocial, "skillSocial");
        }
    }
}