using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HumanStoryteller.Util {
    public abstract class ExtractDefs {
        public static Dictionary<string, Dictionary<string, string>> ExtractCurrentDefs() {
            var result = new Dictionary<string, Dictionary<string, string>> {
                {"DifficultyLevel", ExtractDifficultyLevel()},
                {"ArriveModeTypes", ExtractArriveModeTypes()},
                {"AvailableStats", ExtractAvailableStats()},
                {"Buildings", ExtractBuildings()},
                {"StrategyTypes", ExtractStrategyTypes()},
                {"BodyParts", ExtractBodyParts()},
                {"MentalBreakTypes", ExtractMentalBreakTypes()},
                {"HealthActions", ExtractHealthActions()},
                {"ResearchProject", ExtractResearchProject()},
                {"Stuff", ExtractStuff()},
                {"MineableMaterials", ExtractMineableMaterials()},
                {"PawnKind", ExtractPawnKind()},
                {"Factions", ExtractFactions()},
                {"DiseaseTypes", ExtractDiseaseTypes()},
                {"Biomes", ExtractBiomes()},
                {"Weapons", ExtractWeapons()},
                {"Traders", ExtractTraders()},
                {"ExplosionTypes", ExtractExplosionTypes()},
                {"ThoughtTypes", ExtractThoughtTypes()},
                {"ThingCategories", ExtractThingCategories()},
                {"Plants", ExtractPlants()},
                {"AnimalTypes", ExtractAnimalTypes()},
                {"PawnTraits", ExtractPawnTraits()},
                {"HairTypes", ExtractHairTypes()},
                {"BodyTypes", ExtractBodyTypes()},
                {"Items", ExtractItems()}
            };
            return result;
        }

        private static Dictionary<string, string> ExtractDifficultyLevel() {
            return DefDatabase<DifficultyDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractArriveModeTypes() {
            return DefDatabase<PawnsArrivalModeDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractAvailableStats() {
            return DefDatabase<StatDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);

        }

        private static Dictionary<string, string> ExtractBuildings() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.category.Equals(ThingCategory.Building)).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractStrategyTypes() {
            return DefDatabase<RaidStrategyDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractBodyParts() {
            return DefDatabase<BodyPartDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractMentalBreakTypes() {
            return DefDatabase<MentalBreakDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractHealthActions() {
            return DefDatabase<HediffDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractResearchProject() {
            return DefDatabase<ResearchProjectDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractStuff() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.IsStuff).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractMineableMaterials() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.mineable && def != ThingDefOf.CollapsedRocks && !def.IsSmoothed).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractPawnKind() {
            return DefDatabase<PawnKindDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractFactions() {
            return DefDatabase<FactionDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractDiseaseTypes() {
            return DefDatabase<IncidentDef>.AllDefs.Where(def => def.diseaseIncident != null).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractBiomes() {
            return DefDatabase<BiomeDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractWeapons() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.IsWeapon).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractTraders() {
            return DefDatabase<TraderKindDef>.AllDefs.Where(def => def.label != null).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractExplosionTypes() {
            return DefDatabase<DamageDef>.AllDefs.Where(def => def.isExplosive).ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractThoughtTypes() {
            return DefDatabase<ThoughtDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractThingCategories() {
            return DefDatabase<ThingCategoryDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractPlants() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Plant).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractAnimalTypes() {
            return DefDatabase<PawnKindDef>.AllDefs.Where(def => def.RaceProps.Animal).ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractPawnTraits() {
            return DefDatabase<TraitDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractHairTypes() {
            return DefDatabase<HairDef>.AllDefs.ToDictionary(def => def.defName, def => def.label);
        }

        private static Dictionary<string, string> ExtractBodyTypes() {
            return DefDatabase<BodyTypeDef>.AllDefs.ToDictionary(def => def.defName, def => def.defName);
        }

        private static Dictionary<string, string> ExtractItems() {
            return DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Item).ToDictionary(def => def.defName, def => def.label);
        }
    }
}