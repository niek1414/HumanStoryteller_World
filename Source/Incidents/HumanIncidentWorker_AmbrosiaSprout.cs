using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_AmbrosiaSprout : HumanIncidentWorker {
        public const String Name = "AmbrosiaSprout";

        private static readonly IntRange CountRange = new IntRange(10, 20);

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            IncidentDef def = DefDatabase<IncidentDef>.GetNamed("AmbrosiaSprout");

            if (!(parms is HumanIncidentParams_AmbrosiaSprout)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_AmbrosiaSprout allParams =
                Tell.AssertNotNull((HumanIncidentParams_AmbrosiaSprout) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            ThingDef plant;
            try {
                plant = allParams.PlantKind == ""
                    ? ThingDefOf.Plant_Ambrosia
                    : PlantUtility.ValidPlantTypesForGrowers(new List<IPlantToGrowSettable>()).First(p => p.defName == allParams.PlantKind);
            } catch (InvalidOperationException) {
                plant = ThingDefOf.Plant_Ambrosia;
            }
            
            if (!TryFindRootCell(plant, map, out IntVec3 cell)) {
                return ir;
            }

            Thing thing = null;
            int randomInRange;
            if (allParams.Amount != -1) {
                randomInRange = Mathf.RoundToInt(allParams.Amount);
            } else {
                randomInRange = CountRange.RandomInRange;
            }

            for (int i = 0; i < randomInRange; i++) {
                if (!CellFinder.TryRandomClosewalkCellNear(cell, map, Mathf.RoundToInt(allParams.Range != -1 ? allParams.Range : 6), out IntVec3 result,
                    x => CanSpawnAt(plant, x, map))) {
                    break;
                }

                result.GetPlant(map)?.Destroy();
                Thing thing2 = GenSpawn.Spawn(plant, result, map);
                if (thing == null) {
                    thing = thing2;
                }
            }

            if (thing == null) {
                return ir;
            }

            string title;
            string message;
            if (plant == ThingDefOf.Plant_Ambrosia) {
                title = def.letterLabel;
                message = def.letterText;
            } else {
                title = "PlantSproutTitle".Translate(plant.label.Named("PLANT"));
                message = "PlantSproutMessage".Translate(plant.label.Named("PLANT"));
            }

            SendLetter(allParams, title, message, def.letterDef, thing);
            return ir;
        }

        private bool TryFindRootCell(ThingDef plantDef, Map map, out IntVec3 cell) {
            return CellFinderLoose.TryFindRandomNotEdgeCellWith(10,
                x => CanSpawnAt(plantDef, x, map) && x.GetRoom(map).CellCount >= 64, map, out cell);
        }

        private bool CanSpawnAt(ThingDef plantDef, IntVec3 c, Map map) {
            if (!c.Standable(map) || c.Fogged(map) || map.fertilityGrid.FertilityAt(c) < plantDef.plant.fertilityMin ||
                !c.GetRoom(map).PsychologicallyOutdoors || c.GetEdifice(map) != null) {
                return false;
            }

            Plant plant = c.GetPlant(map);
            if (plant != null && plant.def.plant.growDays > 10f) {
                return false;
            }

            List<Thing> thingList = c.GetThingList(map);
            for (int i = 0; i < thingList.Count; i++) {
                if (thingList[i].def == plantDef) {
                    return false;
                }
            }

            return true;
        }
    }

    public class HumanIncidentParams_AmbrosiaSprout : HumanIncidentParms {
        public float Amount;
        public float Range;
        public string PlantKind;

        public HumanIncidentParams_AmbrosiaSprout() {
        }

        public HumanIncidentParams_AmbrosiaSprout(String target, HumanLetter letter, float amount = -1, float range = -1, string plantKind = "") :
            base(target, letter) {
            Amount = amount;
            Range = range;
            PlantKind = plantKind;
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, Range: {Range}, Kind: {PlantKind}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Amount, "amount");
            Scribe_Values.Look(ref Range, "range");
            Scribe_Values.Look(ref PlantKind, "plantKind");
        }
    }
}