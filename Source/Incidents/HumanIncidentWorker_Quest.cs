using System;
using System.Collections.Generic;
using System.Linq;
using HumanStoryteller.CheckConditions;
using HumanStoryteller.Helper.QuestHelper;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Quest : HumanIncidentWorker {
        public const String Name = "Quest";

        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Quest)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Quest allParams = Tell.AssertNotNull((HumanIncidentParams_Quest) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Map map = (Map) allParams.GetTarget();
            
            SiteCoreDef siteCoreDef = DefDatabase<SiteCoreDef>.GetNamed(allParams.QuestType, false);
            SitePartDef sitePartDef = DefDatabase<SitePartDef>.GetNamed(allParams.ThreatType, false);

            if (siteCoreDef == null) {
                siteCoreDef = SiteCoreDefOf.Nothing;
            }

            var paramsPoints = allParams.Points.GetValue();
            float points = paramsPoints >= 0
                ? StorytellerUtility.DefaultSiteThreatPointsNow() * paramsPoints
                : StorytellerUtility.DefaultSiteThreatPointsNow();

            Faction enemy = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed(allParams.Faction, false));
            Faction ally = null;
            if (allParams.KillReward) {
                ally = FactionUtility.DefaultFactionFrom(DefDatabase<FactionDef>.GetNamed(allParams.RewardFaction, false));
                if (enemy == null || ally == null) {
                    TryFindFactions(out Faction alliedFaction, out Faction enemyFaction);
                    enemy = enemy ?? enemyFaction;
                    ally = ally ?? alliedFaction;
                    if (ally == null) {
                        Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out Faction allyOut, false);
                        ally = allyOut;
                    }
                }
            }

            if (enemy == null) {
                enemy = Find.FactionManager.RandomEnemyFaction();
            }

            TileFinder.TryFindNewSiteTile(out int tile, Mathf.RoundToInt(allParams.MinTileDist.GetValue()),
                Mathf.RoundToInt(allParams.MaxTileDist.GetValue()), false, true, map.Tile);
            Site site = SiteMaker.MakeSite(siteCoreDef, sitePartDef, tile, enemy, true, points);
            site.sitePartsKnown = true;
            if (allParams.KillReward) {
                List<Thing> list = allParams.RewardItem.GetThings(true) ?? GenerateRewards(SiteTuning.BanditCampQuestRewardMarketValueRange, points);
                site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(ally, Mathf.RoundToInt(allParams.RewardFactionRelation.GetValue()), list);
            }

            var duration = allParams.Duration.GetValue();
            if (duration == -1) {
                duration = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
            }

            site.GetComponent<TimeoutComp>().StartTimeout(Mathf.RoundToInt(duration * 60000));
            Pawn pawn;
            IncidentDef def;
            string label = "A location";
            string desc = "A location has been uncovered.";
            switch (siteCoreDef.defName) {
                case "DownedRefugee":
                    pawn = DownedRefugeeQuestUtility.GenerateRefugee(tile);
                    if (allParams.OutName != "") {
                        PawnUtil.SavePawnByName(allParams.OutName, pawn);
                    }

                    PawnUtil.SetDisplayName(pawn, allParams.FirstName, allParams.NickName, allParams.LastName);

                    site.GetComponent<DownedRefugeeComp>().pawn.TryAdd(pawn);
                    def = DefDatabase<IncidentDef>.GetNamed("Quest_DownedRefugee");

                    label = def.letterLabel;
                    desc = def.letterText
                        .Formatted(Mathf.RoundToInt(duration), pawn.ageTracker.AgeBiologicalYears, pawn.story.Title,
                            SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), pawn.Named("PAWN")).AdjustedFor(pawn)
                        .CapitalizeFirst();
                    Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
                    if (mostImportantColonyRelative != null) {
                        PawnRelationDef mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
                        if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0) {
                            pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
                            desc = desc + "\n\n" + "RelatedPawnInvolvedInQuest".Translate(mostImportantColonyRelative.LabelShort,
                                       mostImportantRelation.GetGenderSpecificLabel(pawn), mostImportantColonyRelative.Named("RELATIVE"),
                                       pawn.Named("PAWN")).AdjustedFor(pawn);
                        } else {
                            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref desc, pawn);
                        }

                        label = label + " " + "RelationshipAppendedLetterSuffix".Translate();
                    }

                    if (pawn.relations != null) {
                        pawn.relations.everSeenByPlayer = true;
                    }

                    break;
                case "ItemStash":
                    List<Thing> list = allParams.MapItem.GetThings(true) ?? GenerateRewards(SiteTuning.ItemStashQuestMarketValueRange, points);
                    site.GetComponent<ItemStashContentsComp>().contents.TryAddRangeOrTransfer(list, false);
                    if (ally == null) {
                        ally = Find.FactionManager.RandomNonHostileFaction(false, false, false);
                    }

                    def = DefDatabase<IncidentDef>.GetNamed("Quest_ItemStash");

                    string textTemp = string.Format(def.letterText, ally.leader.LabelShort, ally.def.leaderTitle, ally.Name,
                            GenLabel.ThingsLabel(list), Mathf.RoundToInt(duration).ToString(),
                            SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), GenThing.GetMarketValue(list).ToStringMoney())
                        .CapitalizeFirst();
                    GenThing.TryAppendSingleRewardInfo(ref textTemp, list);
                    desc = textTemp;
                    label = def.letterLabel;
                    break;
                case "PrisonerWillingToJoin":
                    pawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(tile, site.Faction);
                    if (allParams.OutName != "") {
                        PawnUtil.SavePawnByName(allParams.OutName, pawn);
                    }

                    site.GetComponent<PrisonerWillingToJoinComp>().pawn.TryAdd(pawn);

                    def = DefDatabase<IncidentDef>.GetNamed("Quest_PrisonerRescue");
                    desc = def.letterText
                        .Formatted(site.Faction.Name, pawn.ageTracker.AgeBiologicalYears, pawn.story.Title,
                            SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), pawn.Named("PAWN")).AdjustedFor(pawn)
                        .CapitalizeFirst();
                    if (PawnUtility.EverBeenColonistOrTameAnimal(pawn)) {
                        desc = desc + "\n\n" + "PawnWasFormerlyColonist".Translate(pawn.LabelShort, pawn);
                    }

                    PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle(pawn), out string pawnRelationsInfo, true, false);
                    label = def.letterLabel;
                    if (!pawnRelationsInfo.NullOrEmpty()) {
                        string text = desc;
                        desc = text + "\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate(pawn.LabelShort, pawn) + "\n\n" + pawnRelationsInfo;
                        label = label + " " + "RelationshipAppendedLetterSuffix".Translate();
                    }

                    desc = desc + "\n\n" + "PrisonerRescueTimeout".Translate(Mathf.RoundToInt(duration), pawn.LabelShort, pawn.Named("PRISONER"));
                    break;
                case "PreciousLump":
                    site.core.parms.preciousLumpResources =
                        allParams.MineableRock != "" ? ThingDef.Named(allParams.MineableRock) : FindRandomMineableDef();
                    label = "LetterLabelFoundPreciousLump".Translate();
                    desc = "LetterFoundPreciousLump".Translate();
                    break;
            }

            Find.WorldObjects.Add(site);
            if (allParams.MapName != "") {
                MapUtil.SaveMapByName(allParams.MapName, new MapContainer(site));
            }

            if (allParams.OutNames.Count != 0) {
                QuestSitePart part = new QuestSitePart(allParams.OutNames);
                site.parts.Add(part);
            }

            SendLetter(allParams, label, desc, LetterDefOf.PositiveEvent, site, enemy);
            return new IncidentResult_Quest(site, sitePartDef != null);
        }

        private List<Thing> GenerateRewards(FloatRange type, float siteThreatPoints) {
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.totalMarketValueRange = type *
                                          SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints);
            return ThingSetMakerDefOf.Reward_ItemStashQuestContents.root.Generate(parms);
        }

        private ThingDef FindRandomMineableDef() {
            float value = Rand.Value;
            var thingDefs = from x in DefDatabase<ThingDef>.AllDefsListForReading
                where x.mineable && x != ThingDefOf.CollapsedRocks && !x.IsSmoothed
                select x;
            if (value < 0.4f) {
                return (from x in thingDefs
                    where !x.building.isResourceRock
                    select x).RandomElement();
            }

            if (value < 0.75f) {
                return (from x in thingDefs
                    where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
                    select x).RandomElement();
            }

            return (from x in thingDefs
                where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
                select x).RandomElement();
        }

        private void TryFindFactions(out Faction alliedFaction, out Faction enemyFaction) {
            if ((from x in Find.FactionManager.AllFactions
                where !x.def.hidden && !x.defeated && !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) &&
                      CommonHumanlikeEnemyFaction(Faction.OfPlayer, x) != null
                select x).TryRandomElement(out alliedFaction)) {
                enemyFaction = CommonHumanlikeEnemyFaction(Faction.OfPlayer, alliedFaction);
                return;
            }

            alliedFaction = null;
            enemyFaction = null;
        }

        private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2) {
            if ((from x in Find.FactionManager.AllFactions
                where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) && x.HostileTo(f2)
                select x).TryRandomElement(out Faction result)) {
                return result;
            }

            return null;
        }
    }

    public class HumanIncidentParams_Quest : HumanIncidentParms {
        public string QuestType = "";
        public string ThreatType = "";
        public string MapName = "";
        public string OutName = "";
        public string FirstName = "";
        public string NickName = "";
        public string LastName = "";
        public string Faction = "";
        public List<string> OutNames = new List<string>();
        public Number Points = new Number();
        public Number Duration = new Number();
        public Number MinTileDist = new Number(7);
        public Number MaxTileDist = new Number(20);

        public string MineableRock = "";
        public Item MapItem = new Item("", "", "", new Number(20));

        public bool KillReward;
        public Item RewardItem = new Item("", "", "", new Number(10));
        public string RewardFaction = "";
        public Number RewardFactionRelation = new Number(0);

        public HumanIncidentParams_Quest() {
        }

        public HumanIncidentParams_Quest(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, QuestType: [{QuestType}], ThreatType: [{ThreatType}], MapName: [{MapName}], OutName: [{OutName}], FirstName: [{FirstName}], NickName: [{NickName}], LastName: [{LastName}], Faction: [{Faction}], OutNames: [{OutNames.ToCommaList()}], Points: [{Points}], Duration: [{Duration}], MinTileDist: [{MinTileDist}], MaxTileDist: [{MaxTileDist}], MineableRock: [{MineableRock}], MapItem: [{MapItem}], KillReward: [{KillReward}], RewardItem: [{RewardItem}], RewardFaction: [{RewardFaction}], RewardFactionRelation: [{RewardFactionRelation}]";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref QuestType, "questType");
            Scribe_Values.Look(ref ThreatType, "threatType");
            Scribe_Values.Look(ref Faction, "faction");
            Scribe_Values.Look(ref MapName, "mapName");
            Scribe_Values.Look(ref OutName, "name");
            Scribe_Values.Look(ref FirstName, "firstName");
            Scribe_Values.Look(ref NickName, "nickName");
            Scribe_Values.Look(ref LastName, "lastName");
            Scribe_Collections.Look(ref OutNames, "names", LookMode.Value);
            Scribe_Deep.Look(ref Points, "points");
            Scribe_Deep.Look(ref Duration, "duration");
            Scribe_Deep.Look(ref MinTileDist, "minTileDist");
            Scribe_Deep.Look(ref MaxTileDist, "maxTileDist");
            Scribe_Values.Look(ref MineableRock, "mineableRock");
            Scribe_Deep.Look(ref MapItem, "mapItem");
            Scribe_Values.Look(ref KillReward, "killReward");
            Scribe_Deep.Look(ref RewardItem, "rewardItem");
            Scribe_Values.Look(ref RewardFaction, "rewardFaction");
            Scribe_Deep.Look(ref RewardFactionRelation, "rewardFactionRelation");
        }
    }
}