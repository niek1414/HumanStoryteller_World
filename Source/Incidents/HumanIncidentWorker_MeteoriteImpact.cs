using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_MeteoriteImpact : HumanIncidentWorker {
        public const String Name = "MeteoriteImpact";

        public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_MeteoriteImpact)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_MeteoriteImpact allParams =
                Tell.AssertNotNull((HumanIncidentParams_MeteoriteImpact) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            ThingDef mineableDef = allParams.MineableRock != "" ? ThingDef.Named(allParams.MineableRock) : FindRandomMineableDef();
            
            List<Thing> list = null;
            var amount = allParams.Amount.GetValue();
            int num = amount != -1 ? Mathf.RoundToInt(amount) : 1;
            IntVec3 lastCell = IntVec3.Zero;
            for (int i = 0; i < num; i++) {
                if (!TryFindCell(out IntVec3 cell, map)) {
                    continue;
                }

                lastCell = cell;
                list = new List<Thing>();
                int randomInRange =  MineablesCountRange.RandomInRange;
                for (int j = 0; j < randomInRange; j++)
                {
                    Building building = (Building)ThingMaker.MakeThing(mineableDef);
                    building.canChangeTerrainOnDestroyed = false;
                    list.Add(building);
                }
                SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, cell, map);
            }
            
            IncidentDef def = IncidentDef.Named(Name);
            string text = string.Format(def.letterText, list?[0].def.label).CapitalizeFirst();
            SendLetter(allParams, def.letterLabel, text, LetterDefOf.PositiveEvent, lastCell == IntVec3.Zero ? null : new TargetInfo(lastCell, map));
            return ir;
        }

        private bool TryFindCell(out IntVec3 cell, Map map) {
            IntRange mineablesCountRange = ThingSetMaker_Meteorite.MineablesCountRange;
            int maxMineables = mineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false,
                false, true, true, delegate(IntVec3 x) {
                    int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
                    CellRect cellRect = CellRect.CenteredOn(x, num, num);
                    int num2 = 0;
                    CellRect.CellRectIterator iterator = cellRect.GetIterator();
                    while (!iterator.Done()) {
                        if (iterator.Current.InBounds(map) && iterator.Current.Standable(map)) {
                            num2++;
                        }

                        iterator.MoveNext();
                    }

                    return num2 >= maxMineables;
                });
        }
        
        private ThingDef FindRandomMineableDef()
        {
            float value = Rand.Value;
            var thingDefs = from x in DefDatabase<ThingDef>.AllDefsListForReading
                where x.mineable && x != ThingDefOf.CollapsedRocks && !x.IsSmoothed
                select x;
            if (value < 0.4f)
            {
                return (from x in thingDefs
                    where !x.building.isResourceRock
                    select x).RandomElement();
            }
            if (value < 0.75f)
            {
                return (from x in thingDefs
                    where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
                    select x).RandomElement();
            }
            return (from x in thingDefs
                where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
                select x).RandomElement();
        }
    }

    public class HumanIncidentParams_MeteoriteImpact : HumanIncidentParms {
        public Number Amount = new Number();
        public string MineableRock = "";

        public HumanIncidentParams_MeteoriteImpact() {
        }

        public HumanIncidentParams_MeteoriteImpact(string target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Amount: {Amount}, MineableRock: {MineableRock}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Deep.Look(ref Amount, "amount");
            Scribe_Values.Look(ref MineableRock, "mineableRock");
        }
    }
}