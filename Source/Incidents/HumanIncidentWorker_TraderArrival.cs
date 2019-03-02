using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_TraderArrival : HumanIncidentWorker {
        public const String Name = "TraderArrival";

        public override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();

            if (!(parms is HumanIncidentParams_TraderArrival)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_TraderArrival allParams =
                Tell.AssertNotNull((HumanIncidentParams_TraderArrival) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            TraderKindDef kindDef;
            try {
                kindDef = (from x in DefDatabase<TraderKindDef>.AllDefs
                    where x.defName == allParams.TraderKind
                    select x).First();
            } catch (InvalidOperationException) {
                kindDef = null;
            }

            if (kindDef == null) {
                if (!(from x in DefDatabase<TraderKindDef>.AllDefs
                    select x).TryRandomElementByWeight(traderDef => traderDef.CalculatedCommonality, out TraderKindDef result)) {
                    return ir;
                }

                kindDef = result;
            }

            if (kindDef.orbital) {
                TradeShip tradeShip = new TradeShip(kindDef);
                if (map.listerBuildings.allBuildingsColonist.Any(b => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn)) {
                    SendLetter(allParams, tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label),
                        LetterDefOf.PositiveEvent, null);
                }

                map.passingShipManager.AddShip(tradeShip);
                tradeShip.GenerateThings();
            } else {
                if (!CandidateFactions(kindDef, map).TryRandomElement(out var factionResult) &&
                    !CandidateFactions(kindDef, map, true).TryRandomElement(out factionResult)) {
                    return ir;
                }

                var fakeParm = new IncidentParms {
                    faction = factionResult,
                    points = TraderCaravanUtility.GenerateGuardPoints()
                };
                if (!RCellFinder.TryFindRandomPawnEntryCell(out IntVec3 cellResult, map, CellFinder.EdgeRoadChance_Friendly)) {
                    fakeParm.spawnCenter = CellFinder.RandomEdgeCell(map);
                } else {
                    fakeParm.spawnCenter = cellResult;
                }

                fakeParm.forced = true;
                fakeParm.target = map;
                fakeParm.traderKind = kindDef;
                List<Pawn> list = SpawnPawns(map, kindDef, allParams.Names, fakeParm);
                if (list.Count == 0) {
                    return ir;
                }

                foreach (var t in list) {
                    if (t.needs != null && t.needs.food != null) {
                        t.needs.food.CurLevel = t.needs.food.MaxLevel;
                    }
                }

                string letterLabel = "LetterLabelTraderCaravanArrival".Translate(factionResult.Name, kindDef.label).CapitalizeFirst();
                string letterText = "LetterTraderCaravanArrival".Translate(factionResult.Name, kindDef.label).CapitalizeFirst();
                PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText,
                    "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), true);
                SendLetter(allParams, letterLabel, letterText, LetterDefOf.PositiveEvent, list[0], factionResult);
                RCellFinder.TryFindRandomSpotJustOutsideColony(list[0], out IntVec3 result);
                LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(factionResult, result);
                LordMaker.MakeNewLord(factionResult, lordJob, map, list);
            }

            return ir;
        }

        private List<Pawn> SpawnPawns(Map map, TraderKindDef def, List<String> names, IncidentParms parms) {
            PawnGroupMakerParms defaultPawnGroupMakerParms =
                IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDefOf.Trader, parms, true);
            defaultPawnGroupMakerParms.traderKind = def;
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, false).ToList();
            for (var i = 0; i < list.Count; i++) {
                Pawn item = list[i];
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
                GenSpawn.Spawn(item, loc, map);
                if (i < names.Count) {
                    if (item.Name is NameTriple prevNameTriple) {
                        item.Name = new NameTriple(names[i], names[i], prevNameTriple.Last);
                    } else if (item.Name is NameSingle prevNameSingle) {
                        item.Name = new NameTriple(names[i], names[i], prevNameSingle.Name);
                    } else {
                        item.Name = new NameTriple(names[i], names[i], "");
                    }
                }
            }

            return list;
        }

        private IEnumerable<Faction> CandidateFactions(TraderKindDef def, Map map, bool desperate = false) {
            return from f in Find.FactionManager.AllFactions
                where FactionCanBeGroupSource(def, f, map, desperate)
                select f;
        }

        private bool FactionCanBeGroupSource(TraderKindDef def, Faction f, Map map, bool desperate = false) {
            if (f.IsPlayer) {
                return false;
            }
            if (f.def.hidden || f.HostileTo(Faction.OfPlayer) || NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f)) {
                return false;
            }

            if (f.defeated) {
                return false;
            }

            if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) ||
                               !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp))) {
                return false;
            }

            return f.def.caravanTraderKinds.Any() && f.def.caravanTraderKinds.Contains(def);
        }
    }

    public class HumanIncidentParams_TraderArrival : HumanIncidentParms {
        public float Points;
        public string TraderKind;
        public List<String> Names;

        public HumanIncidentParams_TraderArrival() {
        }

        public HumanIncidentParams_TraderArrival(String target, HumanLetter letter, float points = -1, string traderKind = "", List<String> names = null) :
            base(target, letter) {
            Points = points;
            TraderKind = traderKind;
            Names = names ?? new List<string>();
        }

        public override string ToString() {
            return $"{base.ToString()}, Points: {Points}, TraderKind: {TraderKind}, Names: {Names}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Points, "points");
            Scribe_Values.Look(ref TraderKind, "traderKind");
            Scribe_Collections.Look(ref Names, "names", LookMode.Value);
        }
    }
}