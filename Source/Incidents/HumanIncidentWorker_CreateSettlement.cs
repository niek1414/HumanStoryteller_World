using System;
using HumanStoryteller.Model;
using HumanStoryteller.Util;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_CreateSettlement : HumanIncidentWorker {
        public const String Name = "CreateSettlement";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_CreateSettlement)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_CreateSettlement allParams =
                Tell.AssertNotNull((HumanIncidentParams_CreateSettlement) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();

            Faction faction = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed(allParams.Faction, false));
            if (faction == null) {
                faction = Rand.Bool ? Find.FactionManager.RandomNonHostileFaction() : Find.FactionManager.RandomEnemyFaction();
            }

            if (faction == null) {
                faction = Find.FactionManager.AllFactions.RandomElement();
            }
            
            Settlement settlement = (Settlement) WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            settlement.SetFaction(faction);
            int tileResult;
            var site = allParams.Site.GetValue();
            if (site != -1) {
                tileResult = Mathf.RoundToInt(site);
            } else {
                TileFinder.TryFindNewSiteTile(out int tile, 7, 27, false, true, map.Tile);
                tileResult = tile;
            }
            settlement.Tile = tileResult;
            settlement.Name = allParams.SettlementName != "" ? allParams.SettlementName : SettlementNameGenerator.GenerateSettlementName(settlement);
            Find.WorldObjects.Add(settlement);
            
            if (faction.IsPlayer) {
                GetOrGenerateMapUtility.GetOrGenerateMap(settlement.Tile, null);
            }
            
            if (allParams.MapName != "") {
                MapUtil.SaveMapByName(allParams.MapName, settlement);
            }
            
            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_CreateSettlement : HumanIncidentParms {
        public string Faction = "";
        public string MapName = "";
        public Number Site = new Number();
        public string SettlementName = "";

        public HumanIncidentParams_CreateSettlement() {
            Site = new Number();
        }

        public HumanIncidentParams_CreateSettlement(string target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Faction: {Faction}, MapName: {MapName}, Tile: {Site}, SettlementName: {SettlementName}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref MapName, "mapName");
            Scribe_Deep.Look(ref Site, "tile");
            Scribe_Values.Look(ref SettlementName, "settlementName");
        }
    }
}