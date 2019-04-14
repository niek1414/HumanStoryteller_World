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

        public override IncidentResult Execute(HumanIncidentParms parms) {
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
            TileFinder.TryFindNewSiteTile(out int tile, 7, 27, false, true, map.Tile);
            settlement.Tile = tile;
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
        public string Faction;
        public string MapName;
        public string SettlementName;

        public HumanIncidentParams_CreateSettlement() {
        }

        public HumanIncidentParams_CreateSettlement(string target, HumanLetter letter, string faction = "", string mapName = "", string settlementName = "") : base(target, letter) {
            Faction = faction;
            MapName = mapName;
            SettlementName = settlementName;
        }

        public override string ToString() {
            return $"{base.ToString()}, Faction: {Faction}, MapName: {MapName}, SettlementName: {SettlementName}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref MapName, "mapName");
            Scribe_Values.Look(ref SettlementName, "settlementName");
        }
    }
}