using System;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
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
            var container = new MapContainer(settlement);
            if (allParams.PreGenerate) {
                var sizeX = Mathf.RoundToInt(allParams.SiteSizeX.GetValue());
                var sizeY = Mathf.RoundToInt(allParams.SiteSizeY.GetValue());
                if (sizeX == -1) {
                    sizeX = Find.World.info.initialMapSize.x;
                }

                if (sizeY == -1) {
                    sizeY = Find.World.info.initialMapSize.z;
                }

                MapGeneratorDef generator;
                switch (allParams.SiteType) {
                    case "Encounter":
                        generator = MapGeneratorDefOf.Encounter;
                        break;
                    case "SpaceShip":
                        generator = MapGeneratorDefOf.EscapeShip;
                        break;
                    default:
                        generator = settlement.MapGeneratorDef;
                        break;
                }

                var newMap = MapGenerator.GenerateMap(new IntVec3(sizeX, 1, sizeY), settlement, generator, settlement.ExtraGenStepDefs);
                if (allParams.DecoupleNow) {
                    container.Decouple(newMap);
                } else {
                    MapUtil.AddPersistentMap(newMap);
                }
            }

            if (allParams.MapName != "") {
                MapUtil.SaveMapByName(allParams.MapName, container);
            }
                        
            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_CreateSettlement : HumanIncidentParms {
        public string Faction = "";
        public string MapName = "";
        public Number Site = new Number();
        public Number SiteSizeX = new Number();
        public Number SiteSizeY = new Number();
        public string SettlementName = "";
        public string SiteType = "";
        public bool PreGenerate;
        public bool DecoupleNow;

        public HumanIncidentParams_CreateSettlement() {
            Site = new Number();
        }

        public HumanIncidentParams_CreateSettlement(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, Faction: [{Faction}], MapName: [{MapName}], Site: [{Site}], SiteSizeX: [{SiteSizeX}], SiteSizeY: [{SiteSizeY}], SettlementName: [{SettlementName}], SiteType: [{SiteType}], PreGenerate: [{PreGenerate}], DecoupleNow: [{DecoupleNow}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref MapName, "mapName");
            Scribe_Deep.Look(ref Site, "tile");
            Scribe_Deep.Look(ref SiteSizeX, "siteSizeX");
            Scribe_Deep.Look(ref SiteSizeY, "siteSizeY");
            Scribe_Values.Look(ref SettlementName, "settlementName");
            Scribe_Values.Look(ref PreGenerate, "preGenerate");
            Scribe_Values.Look(ref DecoupleNow, "decoupleNow");
        }
    }
}