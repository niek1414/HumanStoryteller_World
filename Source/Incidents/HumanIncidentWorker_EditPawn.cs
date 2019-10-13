using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.PawnGroup;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_EditPawn : HumanIncidentWorker {
        public const String Name = "EditPawn";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_EditPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_EditPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_EditPawn) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            foreach (var pawn in allParams.Names.Filter(map)) {
                if (pawn.DestroyedOrNull() || pawn.Dead) {
                    continue;
                }

                if (pawn.Spawned && allParams.Despawn) {
                    pawn.DeSpawn();
                } else if (!pawn.Spawned && pawn.holdingOwner == null && !allParams.Despawn) {
                    pawn.SpawnSetup(map, false);
                }

                PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);

                if (allParams.Strip) {
                    pawn.Strip();
                }

                if (allParams.ClearMind) {
                    pawn.jobs.ClearQueuedJobs();
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
                    pawn.ClearMind();
                }

                pawn.drafter.Drafted = allParams.SetDrafted;

                if (allParams.Faction != "") {
                    pawn.SetFaction(FactionUtility.DefaultFactionFrom(FactionDef.Named(allParams.Faction)));
                }

                if (allParams.Traits.Count > 0) {
                    pawn.story?.traits?.allTraits?.Clear();
                }

                allParams.Traits.ForEach(s => {
                    var split = s.Split('|');
                    var traitDef = DefDatabase<TraitDef>.GetNamed(split[0], false);
                    if (traitDef == null) {
                        Tell.Warn("Did not find trait with name: " + split[0]);
                        return;
                    }

                    TraitDegreeData data;
                    try {
                        data = traitDef.DataAtDegree(Convert.ToInt32(split[1]));
                    } catch (ArgumentOutOfRangeException) {
                        Tell.Warn("Did not find correct trait degree");
                        return;
                    }

                    pawn.story?.traits?.GainTrait(new Trait(traitDef, data?.degree ?? 0));
                });
                //TODO gear
                //TODO finish item object that is already created in the storycreator
//                pawn.

                var cell = allParams.Location.GetSingleCell(map, false);
                if (cell.IsValid) {
                    pawn.Position = cell;
                    pawn.Notify_Teleported(true, false);
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
        public Number SkillAnimals = new Number();
        public Number SkillArtistic = new Number();
        public Number SkillConstruction = new Number();
        public Number SkillCooking = new Number();
        public Number SkillCrafting = new Number();
        public Number SkillPlants = new Number();
        public Number SkillMedicine = new Number();
        public Number SkillMelee = new Number();
        public Number SkillMining = new Number();
        public Number SkillIntellectual = new Number();
        public Number SkillShooting = new Number();
        public Number SkillSocial = new Number();
        public Number AgeBioYear = new Number();

        public bool SkillAdd;

        public List<String> Traits = new List<string>();
        public PawnGroupSelector Names = new PawnGroupSelector();
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";
        public bool Despawn;
        public bool Strip;
        public bool ClearMind;
        public bool Banish;
        public bool SetDrafted;
        public string Faction = "";

        public Location Location = new Location();

        public HumanIncidentParams_EditPawn() {
        }

        public HumanIncidentParams_EditPawn(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, SkillAnimals: {SkillAnimals}, SkillArtistic: {SkillArtistic}, SkillConstruction: {SkillConstruction}, SkillCooking: {SkillCooking}, SkillCrafting: {SkillCrafting}, SkillPlants: {SkillPlants}, SkillMedicine: {SkillMedicine}, SkillMelee: {SkillMelee}, SkillMining: {SkillMining}, SkillIntellectual: {SkillIntellectual}, SkillShooting: {SkillShooting}, SkillSocial: {SkillSocial}, AgeBioYear: {AgeBioYear}, SkillAdd: {SkillAdd}, Traits: {Traits}, Names: {Names}, FirstName: {FirstName}, NickName: {NickName}, LastName: {LastName}, Despawn: {Despawn}, Strip: {Strip}, ClearMind: {ClearMind}, Banish: {Banish}, SetDrafted: {SetDrafted}, Faction: {Faction}, Location: {Location}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Names, "names");
            Scribe_Values.Look(ref FirstName, "firstName");
            Scribe_Values.Look(ref NickName, "nickName");
            Scribe_Values.Look(ref LastName, "lastName");
            Scribe_Values.Look(ref Despawn, "despawn");
            Scribe_Values.Look(ref Strip, "strip");
            Scribe_Values.Look(ref ClearMind, "clearMind");
            Scribe_Values.Look(ref Banish, "banish");
            Scribe_Values.Look(ref SetDrafted, "setDrafted");
            Scribe_Deep.Look(ref AgeBioYear, "ageBioYear");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Deep.Look(ref Location, "location");
            Scribe_Collections.Look(ref Traits, "traits", LookMode.Value);

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