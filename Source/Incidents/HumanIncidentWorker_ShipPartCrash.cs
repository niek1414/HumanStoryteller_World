using System;
using System.Collections.Generic;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_ShipPartCrash : HumanIncidentWorker {
        public const String Name = "ShipPartCrash";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_ShipPartCrash)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_ShipPartCrash
                allParams = Tell.AssertNotNull((HumanIncidentParams_ShipPartCrash) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            ThingDef part;
            if (allParams.ShipCrashedPart != "") {
                part = ThingDef.Named(allParams.ShipCrashedPart);
            } else {
                part = ThingDefOf.CrashedPsychicEmanatorShipPart;
            }

            var amount = allParams.Amount.GetValue();
            int countToSpawn = Mathf.RoundToInt(amount);

            List<TargetInfo> list = new List<TargetInfo>();
            float shrapnelDirection = Rand.Range(0f, 360f);
            for (int i = 0; i < countToSpawn; i++) {
                if (!CellFinderLoose.TryFindSkyfallerCell(
                    part == ThingDefOf.ShipChunk ? ThingDefOf.ShipChunkIncoming : ThingDefOf.CrashedShipPartIncoming, map, out var cell, 14,
                    default(IntVec3), -1, false, true, true, true)) {
                    break;
                }

                if (part == ThingDefOf.ShipChunk) {
                    SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, cell, map);
                } else {
                    Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart) ThingMaker.MakeThing(part);
                    building_CrashedShipPart.SetFaction(Faction.OfMechanoids);
                    building_CrashedShipPart.GetComp<CompSpawnerMechanoidsOnDamaged>().pointsLeft =
                        Mathf.Max(StorytellerUtility.DefaultThreatPointsNow(map) * 0.9f, 300f);
                    Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, building_CrashedShipPart);
                    skyfaller.shrapnelDirection = shrapnelDirection;
                    GenSpawn.Spawn(skyfaller, cell, map);
                }
                list.Add(new TargetInfo(cell, map));
            }

            if (part == ThingDefOf.ShipChunk) {
                SendLetter(allParams, ThingDefOf.ShipChunk.label, "MessageShipChunkDrop".Translate(), LetterDefOf.PositiveEvent, list);
            } else {
                SendLetter(allParams, part.label, part.description, LetterDefOf.ThreatSmall, list);
            }

            return ir;
        }
    }

    public class HumanIncidentParams_ShipPartCrash : HumanIncidentParms {
        public Number Amount = new Number(1);
        public string ShipCrashedPart = "";

        public HumanIncidentParams_ShipPartCrash() {
        }

        public HumanIncidentParams_ShipPartCrash(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, ShipCrashedPart: {ShipCrashedPart}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref ShipCrashedPart, "shipCrashedPart");
        }
    }
}