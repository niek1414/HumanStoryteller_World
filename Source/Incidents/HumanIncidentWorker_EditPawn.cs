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
                        case "Random":
                            intVec = DropCellFinder.RandomDropSpot(map);
                            break;
                        default:
                            intVec = DropCellFinder.RandomDropSpot(map);
                            break;
                    }

                    pawn.Position = intVec;
                }
                
                if (allParams.SetDrafted) {
                    pawn.drafter.Drafted = true;
                }
                
                if (allParams.Banish) {
                    PawnBanishUtility.Banish(pawn);
                }

                var bioYear = allParams.AgeBioYear.GetValue();
                if (bioYear != -1) {
                    pawn.ageTracker.AgeBiologicalTicks = Mathf.RoundToInt(bioYear * 3600000L);
                }

                var skillAnimals = allParams.SkillAnimals.GetValue();
                if (skillAnimals != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Animals).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Animals).Level : 0) + skillAnimals);
                }

                var skillArtistic = allParams.SkillArtistic.GetValue();
                if (skillArtistic != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Artistic).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Artistic).Level : 0) + skillArtistic);
                }

                var skillConstruction = allParams.SkillConstruction.GetValue();
                if (skillConstruction != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Construction).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Construction).Level : 0) +
                                         skillConstruction);
                }

                var skillCooking = allParams.SkillCooking.GetValue();
                if (skillCooking != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Cooking).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Cooking).Level : 0) + skillCooking);
                }

                var skillCrafting = allParams.SkillCrafting.GetValue();
                if (skillCrafting != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Crafting).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Crafting).Level : 0) + skillCrafting);
                }

                var skillPlants = allParams.SkillPlants.GetValue();
                if (skillPlants != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Plants).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Plants).Level : 0) + skillPlants);
                }

                var skillMedicine = allParams.SkillMedicine.GetValue();
                if (skillMedicine != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Medicine).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Medicine).Level : 0) + skillMedicine);
                }

                var skillMelee = allParams.SkillMelee.GetValue();
                if (skillMelee != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Melee).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Melee).Level : 0) + skillMelee);
                }

                var skillMining = allParams.SkillMining.GetValue();
                if (skillMining != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Mining).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Mining).Level : 0) + skillMining);
                }

                var skillIntellectual = allParams.SkillIntellectual.GetValue();
                if (skillIntellectual != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Intellectual).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Intellectual).Level : 0) +
                                         skillIntellectual);
                }

                var skillShooting = allParams.SkillShooting.GetValue();
                if (skillShooting != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Shooting).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Shooting).Level : 0) + skillShooting);
                }

                var skillSocial = allParams.SkillSocial.GetValue();
                if (skillSocial != -1) {
                    pawn.skills.GetSkill(SkillDefOf.Social).Level =
                        Mathf.RoundToInt((allParams.SkillAdd ? pawn.skills.GetSkill(SkillDefOf.Social).Level : 0) + skillSocial);
                }
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_EditPawn : HumanIncidentParms {
        public Number SkillAnimals;
        public Number SkillArtistic;
        public Number SkillConstruction;
        public Number SkillCooking;
        public Number SkillCrafting;
        public Number SkillPlants;
        public Number SkillMedicine;
        public Number SkillMelee;
        public Number SkillMining;
        public Number SkillIntellectual;
        public Number SkillShooting;
        public Number SkillSocial;
        public Number AgeBioYear;

        public bool SkillAdd;

        public List<String> Names;
        public bool Strip;
        public bool ClearMind;
        public bool Banish;
        public bool SetDrafted;
        public string Faction;

        public string Location;

        public HumanIncidentParams_EditPawn() {
        }

        public HumanIncidentParams_EditPawn(string target, HumanLetter letter, Number skillAnimals, Number skillArtistic, Number skillConstruction,
            Number skillCooking, Number skillCrafting, Number skillPlants, Number skillMedicine, Number skillMelee, Number skillMining,
            Number skillIntellectual, Number skillShooting, Number skillSocial, Number ageBioYear, bool skillAdd, List<string> names, bool strip,
            bool clearMind, bool banish, bool drafted, string faction, string location) : base(target, letter) {
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
            AgeBioYear = ageBioYear;
            SkillAdd = skillAdd;
            Names = names ?? new List<string>();;
            Strip = strip;
            ClearMind = clearMind;
            Banish = banish;
            SetDrafted = drafted;
            Faction = faction;
            Location = location;
        }

        public HumanIncidentParams_EditPawn(string target, HumanLetter letter, bool skillAdd = false, List<string> names = null, bool strip = false,
            bool clearMind = false, bool banish = false, bool drafted = false, string faction = "", string location = "") : this(target, letter,
            new Number(), new Number(), new Number(), new Number(), new Number(), new Number(), new Number(), new Number(), new Number(),
            new Number(), new Number(), new Number(), new Number(), skillAdd, names, strip, clearMind, banish, drafted, faction, location) {
        }

        public override string ToString() {
            return $"{base.ToString()}, SkillAnimals: {SkillAnimals}, SkillArtistic: {SkillArtistic}, SkillConstruction: {SkillConstruction}, SkillCooking: {SkillCooking}, SkillCrafting: {SkillCrafting}, SkillPlants: {SkillPlants}, SkillMedicine: {SkillMedicine}, SkillMelee: {SkillMelee}, SkillMining: {SkillMining}, SkillIntellectual: {SkillIntellectual}, SkillShooting: {SkillShooting}, SkillSocial: {SkillSocial}, AgeBioYear: {AgeBioYear}, SkillAdd: {SkillAdd}, Names: {Names}, Strip: {Strip}, ClearMind: {ClearMind}, Banish: {Banish}, SetDrafted: {SetDrafted}, Faction: {Faction}, Location: {Location}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
            Scribe_Values.Look(ref Strip, "strip");
            Scribe_Values.Look(ref ClearMind, "clearMind");
            Scribe_Values.Look(ref Banish, "banish");
            Scribe_Values.Look(ref SetDrafted, "setDrafted");
            Scribe_Deep.Look(ref AgeBioYear, "ageBioYear");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref Location, "location");

            Scribe_Values.Look(ref SkillAdd, "skillAdd");
            Scribe_Deep.Look(ref SkillAnimals, "skillAnimals");
            Scribe_Deep.Look(ref SkillArtistic, "skillArtistic");
            Scribe_Deep.Look(ref SkillConstruction, "skillConstruction");
            Scribe_Deep.Look(ref SkillCooking, "skillCooking");
            Scribe_Deep.Look(ref SkillCrafting, "skillCrafting");
            Scribe_Deep.Look(ref SkillPlants, "skillPlants");
            Scribe_Deep.Look(ref SkillMedicine, "skillMedicine");
            Scribe_Deep.Look(ref SkillMelee, "skillMelee");
            Scribe_Deep.Look(ref SkillMining, "skillMining");
            Scribe_Deep.Look(ref SkillIntellectual, "skillIntellectual");
            Scribe_Deep.Look(ref SkillShooting, "skillShooting");
            Scribe_Deep.Look(ref SkillSocial, "skillSocial");
        }
    }
}