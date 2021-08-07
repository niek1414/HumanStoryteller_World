using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Model.Incident;
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

        protected override IncidentResult Execute(HumanIncidentParams @params) {
            IncidentResult ir = new IncidentResult();

            if (!(@params is HumanIncidentParams_EditPawn)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + @params.GetType());
                return ir;
            }

            HumanIncidentParams_EditPawn allParams =
                Tell.AssertNotNull((HumanIncidentParams_EditPawn) @params, nameof(@params), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            foreach (var pawn in allParams.Pawns.Filter(map)) {Tell.Debug("a");
                var graphicsChanged = false;

                if (pawn.DestroyedOrNull() || pawn.Dead || pawn.NonHumanlikeOrWildMan()) {
                    continue;
                }

                if (pawn.Spawned && allParams.Despawn) {
                    pawn.DeSpawn();
                } else if (!pawn.Spawned && pawn.holdingOwner == null && !allParams.Despawn) {
                    pawn.SpawnSetup(map, false);
                }

                PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);

                if (allParams.HairType != "") {
                    var hairDef = DefDatabase<HairDef>.GetNamed(allParams.HairType, false);
                    if (hairDef != null) {
                        if (pawn.story == null) {
                            pawn.story = new Pawn_StoryTracker(pawn);
                        }
                        pawn.story.hairDef = hairDef;
                        graphicsChanged = true;
                    } else {
                        Tell.Warn("Did not find hair def with name: " + allParams.HairType);
                    }
                }

                if (allParams.HairColor != "") {
                    var optColor = PawnUtil.HexToColor(allParams.HairColor);
                    if (optColor.HasValue) {
                        if (pawn.story == null) {
                            pawn.story = new Pawn_StoryTracker(pawn);
                        }
                        pawn.story.hairColor = optColor.Value;
                        graphicsChanged = true;
                    } else {
                        Tell.Log("Tried to set hair color but could not do to the warning above.");
                    }
                }

                if (allParams.BodyType != "") {
                    var bodyTypeDef = DefDatabase<BodyTypeDef>.GetNamed(allParams.BodyType, false);
                    if (bodyTypeDef != null) {
                        if (pawn.story == null) {
                            pawn.story = new Pawn_StoryTracker(pawn);
                        }
                        pawn.story.bodyType = bodyTypeDef;
                        graphicsChanged = true;
                    } else {
                        Tell.Warn("Did not find body type def with name: " + allParams.BodyType);
                    }
                }

                var melanin = allParams.Melanin.GetValue();
                if (melanin != -1) {
                    if (pawn.story == null) {
                        pawn.story = new Pawn_StoryTracker(pawn);
                    }
                    pawn.story.melanin = melanin;
                    graphicsChanged = true;
                }
                
                if (allParams.Strip) {
                    pawn.Strip();
                }

                if (allParams.ClearMind) {
                    pawn.jobs?.ClearQueuedJobs();
                    pawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
                    pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.LeftVoluntarily);
                    pawn.ClearMind();
                }

                if (pawn.drafter == null) {
                    pawn.drafter = new Pawn_DraftController(pawn);
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

                allParams.Gear.ForEach(item => {
                    if (!item.NotEmpty() || item.Thing.Equals("None")) return;
                    var thing = item.GetThing();
                    if (thing == null) return;
                    try {
                        if (thing.def.IsApparel && ApparelUtility.HasPartsToWear(pawn, thing.def)) {
                            pawn.apparel.Wear(thing as Apparel, false);
                        } else if (thing.def.equipmentType == EquipmentType.Primary) {
                            if (pawn.equipment.Primary != null) {
                                pawn.equipment.Remove(pawn.equipment.Primary);
                            }

                            pawn.equipment.AddEquipment(thing);
                        } else {
                            pawn.inventory.innerContainer.TryAdd(thing);
                        }
                    } catch (Exception e) {
                        Tell.Warn(
                            "Pawn (" + pawn.Name + ", " + pawn.kindDef.defName + ") cannot wear, equip or add " + thing.def.defName + " (" +
                            thing.Stuff.defName + ") to inventory", e.Message, e.StackTrace);
                    }
                });

                var cell = allParams.Location.GetSingleCell(map, false);
                if (cell.IsValid) {
                    pawn.Position = cell;
                    pawn.Notify_Teleported(true, false);
                }

                if (allParams.Banish) {
                    PawnBanishUtility.Banish(pawn);
                }
                
                if (allParams.RestrictedArea.isSet()) {
                    if (pawn.playerSettings == null) {
                        pawn.playerSettings = new Pawn_PlayerSettings(pawn);
                    }
                    var areaAllowed = new Area_Allowed(map.areaManager, "RestrictedArea" + pawn.LabelShort + Rand.Int);
                    pawn.playerSettings.AreaRestriction = areaAllowed;
                    allParams.RestrictedArea.GetZone(map).Cells.ForEach(zoneCell => {
                        var intVec3 = zoneCell.Pos;
                        if (intVec3.IsValid && intVec3.InBounds(map)) {
                            areaAllowed[intVec3] = true;
                        }
                    });
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

                if (graphicsChanged) {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }

            SendLetter(@params);

            return ir;
        }
    }

    public class HumanIncidentParams_EditPawn : HumanIncidentParams {
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

        public string BodyType = "";
        public Number Melanin = new Number();
        public string HairType = "";
        public string HairColor = "";

        public bool SkillAdd;

        public List<String> Traits = new List<string>();
        public PawnGroupSelector Pawns = new PawnGroupSelector();
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";
        public bool Despawn;
        public bool Strip;
        public bool ClearMind;
        public bool Banish;
        public bool SetDrafted;
        public string Faction = "";
        public List<Item> Gear = new List<Item>();

        public Location Location = new Location();
        public Location RestrictedArea = new Location();

        public HumanIncidentParams_EditPawn() {
        }

        public HumanIncidentParams_EditPawn(Target target, HumanLetter letter) : base(target, letter) {
        }


        public override string ToString() {
            return $"{base.ToString()}, SkillAnimals: [{SkillAnimals}], SkillArtistic: [{SkillArtistic}], SkillConstruction: [{SkillConstruction}], SkillCooking: [{SkillCooking}], SkillCrafting: [{SkillCrafting}], SkillPlants: [{SkillPlants}], SkillMedicine: [{SkillMedicine}], SkillMelee: [{SkillMelee}], SkillMining: [{SkillMining}], SkillIntellectual: [{SkillIntellectual}], SkillShooting: [{SkillShooting}], SkillSocial: [{SkillSocial}], AgeBioYear: [{AgeBioYear}], BodyType: [{BodyType}], Melanin: [{Melanin}], HairType: [{HairType}], HairColor: [{HairColor}], SkillAdd: [{SkillAdd}], Traits: [{Traits.ToStringSafeEnumerable()}], Pawns: [{Pawns}], FirstName: [{FirstName}], NickName: [{NickName}], LastName: [{LastName}], Despawn: [{Despawn}], Strip: [{Strip}], ClearMind: [{ClearMind}], Banish: [{Banish}], SetDrafted: [{SetDrafted}], Faction: [{Faction}], Gear: [{Gear.ToStringSafeEnumerable()}], Location: [{Location}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Pawns, "names");
            Scribe_Values.Look(ref FirstName, "firstName");
            Scribe_Values.Look(ref NickName, "nickName");
            Scribe_Values.Look(ref LastName, "lastName");

            Scribe_Values.Look(ref BodyType, "bodyType");
            Scribe_Deep.Look(ref Melanin, "melanin");
            Scribe_Values.Look(ref HairType, "hairType");
            Scribe_Values.Look(ref HairColor, "hairColor");

            Scribe_Values.Look(ref Despawn, "despawn");
            Scribe_Values.Look(ref Strip, "strip");
            Scribe_Values.Look(ref ClearMind, "clearMind");
            Scribe_Values.Look(ref Banish, "banish");
            Scribe_Values.Look(ref SetDrafted, "setDrafted");
            Scribe_Deep.Look(ref AgeBioYear, "ageBioYear");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Deep.Look(ref Location, "location");
            Scribe_Deep.Look(ref RestrictedArea, "restrictedArea");
            Scribe_Collections.Look(ref Traits, "traits", LookMode.Value);
            Scribe_Collections.Look(ref Gear, "gear", LookMode.Deep);

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