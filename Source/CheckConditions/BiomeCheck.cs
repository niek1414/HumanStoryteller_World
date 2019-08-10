using System;
using System.Collections.Generic;
using HumanStoryteller.Incidents;
using HumanStoryteller.Util;
using Verse;

namespace HumanStoryteller.CheckConditions {
    public class BiomeCheck : CheckCondition {
        public const String Name = "Biome";
        private List<string> _biomes;

        public BiomeCheck() {
        }

        public BiomeCheck(List<string> biomes) {
            _biomes = Tell.AssertNotNull(biomes, nameof(biomes), GetType().Name);
        }

        public override bool Check(IncidentResult result, int checkPosition) {
            return _biomes.Contains(result.Target.GetMapFromTarget().Biome.defName);
        }

        public override string ToString() {
            return $"Biomes: {_biomes}";
        }

        public override void ExposeData() {
            base.ExposeData();
            Scribe_Collections.Look(ref _biomes, "biomes");
        }
    }
}