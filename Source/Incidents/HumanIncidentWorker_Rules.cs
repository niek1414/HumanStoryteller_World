using System;
using System.Collections.Generic;
using HarmonyLib;
using HumanStoryteller.Model;
using HumanStoryteller.Model.StoryPart;
using HumanStoryteller.Util;
using HumanStoryteller.Util.Logging;
using RimWorld;
using Verse;

namespace HumanStoryteller.Incidents {
    class HumanIncidentWorker_Rules : HumanIncidentWorker {
        public const String Name = "Rules";


        protected override IncidentResult Execute(HumanIncidentParms parms) {
            IncidentResult ir = new IncidentResult();
            if (!(parms is HumanIncidentParams_Rules)) {
                Tell.Err("Tried to execute " + GetType() + " but param type was " + parms.GetType());
                return ir;
            }

            HumanIncidentParams_Rules allParams = Tell.AssertNotNull((HumanIncidentParams_Rules) parms, nameof(parms), GetType().Name);
            Tell.Log($"Executing event {Name} with:{allParams}");

            Traverse.Create(Current.Game.Rules).Field("disallowedBuildings").SetValue(new List<BuildableDef>());

            foreach (var building in allParams.DisallowedBuildings) {
                Current.Game.Rules.SetAllowBuilding(ThingDef.Named(building), false);
            }

            Traverse.Create(Current.Game.Rules).Field("disallowedDesignatorTypes").SetValue(new List<Type>());

            foreach (var designator in allParams.DisallowedDesignators) {
                switch (designator) {
                    case "Designator_ZoneAdd_Growing":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_ZoneAdd_Growing), false);
                        break;

                    case "Designator_Mine":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Mine), false);
                        break;

                    case "Designator_Hunt":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Hunt), false);
                        break;

                    case "Designator_Tame":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Tame), false);
                        break;

                    case "Designator_Build":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Build), false);
                        break;

                    case "Designator_Claim":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Claim), false);
                        break;

                    case "Designator_Slaughter":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Slaughter), false);
                        break;

                    case "Designator_Haul":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Haul), false);
                        break;

                    case "Designator_Strip":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_Strip), false);
                        break;

                    case "Designator_ZoneAddStockpile_Resources":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_ZoneAddStockpile_Resources), false);
                        break;

                    case "Designator_ZoneAddStockpile_Dumping":
                        Current.Game.Rules.SetAllowDesignator(typeof(Designator_ZoneAddStockpile_Dumping), false);
                        break;
                }
            }

            if (allParams.ExplodeOnDeath) {
                ScenPart_OnPawnDeathExplode explode = new ScenPart_OnPawnDeathExplode();
                var radius = allParams.ExplodeRadius.GetValue();
                if (radius != -1) {
                    Traverse.Create(explode).Field("radius").SetValue(radius);
                }

                ScenarioEditorUtil.AddPart(Current.Game.Scenario, explode);
            } else {
                ScenarioEditorUtil.RemovePart(Current.Game.Scenario, typeof(ScenPart_OnPawnDeathExplode));
            }

            foreach (KeyValuePair<string, Number> stat in allParams.Stats) {
                var statDef = StatDef.Named(stat.Key);
                if (statDef == null) continue;

                ScenarioEditorUtil.RemoveStatPart(Current.Game.Scenario, statDef);

                var part = new ScenPart_StatFactor();
                Traverse.Create(part).Field("stat").SetValue(statDef);
                Traverse.Create(part).Field("factor").SetValue(stat.Value.GetValue());
                ScenarioEditorUtil.AddPart(Current.Game.Scenario, part);
            }

            SendLetter(parms);

            return ir;
        }
    }

    public class HumanIncidentParams_Rules : HumanIncidentParms {
        public List<string> DisallowedBuildings = new List<string>();
        public List<string> DisallowedDesignators = new List<string>();
        public bool ExplodeOnDeath;
        public Number ExplodeRadius = new Number();
        public Dictionary<string, Number> Stats = new Dictionary<string, Number>();

        public HumanIncidentParams_Rules() {
        }

        public HumanIncidentParams_Rules(Target target, HumanLetter letter) : base(target, letter) {
        }

        public override string ToString() {
            return $"{base.ToString()}, DisallowedBuildings: {DisallowedBuildings.Join()}, DisallowedDesignators: {DisallowedDesignators.Join()}, ExplodeOnDeath: {ExplodeOnDeath}, ExplodeRadius: {ExplodeRadius}, Stats: {Stats.Join()}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref DisallowedBuildings, "disallowedBuildings", LookMode.Value);
            Scribe_Collections.Look(ref DisallowedDesignators, "disallowedDesignators", LookMode.Value);
            Scribe_Collections.Look(ref Stats, "stats", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref ExplodeOnDeath, "explodeOnDeath");
            Scribe_Deep.Look(ref ExplodeRadius, "explodeRadius");
        }
    }
}