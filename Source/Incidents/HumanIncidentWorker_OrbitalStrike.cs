using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_OrbitalStrike : HumanIncidentWorker {
        public const String Name = "OrbitalStrike";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_OrbitalStrike)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_OrbitalStrike allParams =
                Tell.AssertNotNull((HumanIncidentParams_OrbitalStrike) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            IntVec3 cell;
            if (allParams.Name != "") {
                var pawn = PawnUtil.GetPawnByName(allParams.Name);

                if (pawn != null) {
                    cell = pawn.Position;
                } else {
                    cell = DropCellFinder.RandomDropSpot(map);
                }
            } else {
                cell = DropCellFinder.RandomDropSpot(map);
            }
            
            switch (allParams.OrbitalType) {
                case "Bombardment":
                    Bombardment bombardment = (Bombardment)GenSpawn.Spawn(ThingDefOf.Bombardment, cell, map);
                    bombardment.duration = 540;
                    bombardment.instigator = null;
                    bombardment.weaponDef = null;
                    bombardment.StartStrike();
                    break;
                case "PowerBeam":
                    PowerBeam powerBeam = (PowerBeam)GenSpawn.Spawn(ThingDefOf.PowerBeam, cell, map);
                    powerBeam.duration = 600;
                    powerBeam.instigator = null;
                    powerBeam.weaponDef = null;
                    powerBeam.StartStrike();
                    break;
                default:
                    Thing thing = ThingMaker.MakeThing(ThingDef.Named(allParams.OrbitalType));
                    GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near);
                    break;
            }
            
            if (parms.Letter?.Type != null) {
                if (parms.Letter.Shake) {
                    Find.CameraDriver.shaker.DoShake(4f);
                }

                Find.LetterStack.ReceiveLetter(LetterMaker.MakeLetter(parms.Letter.Title, parms.Letter.Message, parms.Letter.Type));
            }

            return ir;
        }
    }

    public class HumanIncidentParams_OrbitalStrike : HumanIncidentParms {
        public string Name;
        public string OrbitalType;

        public HumanIncidentParams_OrbitalStrike() {
        }

        public HumanIncidentParams_OrbitalStrike(String target, HumanLetter letter, string name = "", string orbitalType = "") :
            base(target, letter) {
            Name = name;
            OrbitalType = orbitalType;
        }

        public override string ToString() {
            return $"{base.ToString()}, Name: {Name}, OrbitalType: {OrbitalType}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Name, "name");
            Scribe_Values.Look(ref OrbitalType, "orbitalType");
        }
    }
}