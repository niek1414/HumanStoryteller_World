using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CropBlight : HumanIncidentWorker {
        public const String Name = "CropBlight";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_CropBlight)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_CropBlight allParams =
                Tell.AssertNotNull((HumanIncidentParams_CropBlight) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            if (!TryFindRandomBlightablePlant(map, out Plant plant))
            {
                return ir;
            }
            Room room = plant.GetRoom();
            plant.CropBlighted();
            int i = 0;
            for (int num = GenRadial.NumCellsInRadius(allParams.Radius); i < num; i++)
            {
                IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map) && intVec.GetRoom(map) == room)
                {
                    Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
                    if (firstBlightableNowPlant != null && firstBlightableNowPlant != plant && Rand.Chance(allParams.Chance * BlightChanceFactor(firstBlightableNowPlant.Position, plant.Position)))
                    {
                        firstBlightableNowPlant.CropBlighted();
                    }
                }
            }
            Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), "LetterCropBlight".Translate(), LetterDefOf.NegativeEvent, new TargetInfo(plant.Position, map));
            return ir;
        }

        private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
        {
            Thing result;
            bool result2 = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                where ((Plant)x).BlightableNow
                select x).TryRandomElement(out result);
            plant = (Plant)result;
            return result2;
        }

        private float BlightChanceFactor(IntVec3 c, IntVec3 root)
        {
            return Mathf.InverseLerp(15f, 7.5f, c.DistanceTo(root));
        }
    }

    public class HumanIncidentParams_CropBlight : HumanIncidentParms {
        public float Radius;
        public float Chance;

        public HumanIncidentParams_CropBlight() {
        }

        public HumanIncidentParams_CropBlight(String target, HumanLetter letter, float radius = 15, float chance = 0.4f) :
            base(target, letter) {
            Radius = radius;
            Chance = chance;
        }

        public override string ToString() {
            return $"{base.ToString()}, Radius: {Radius}, Chance: {Chance}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Radius, "radius");
            Scribe_Values.Look(ref Chance, "chance");
        }
    }
}