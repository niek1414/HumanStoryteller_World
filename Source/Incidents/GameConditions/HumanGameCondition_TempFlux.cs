using RimWorld;

namespace HumanStoryteller.Incidents.GameConditions {
    public class HumanGameCondition_TempFlux : GameCondition {
        public int LerpTicks = 12000;
        public float MaxTempOffset = -20f;

        public override float TemperatureOffset() {
            return GameConditionUtility.LerpInOutValue(this, LerpTicks, MaxTempOffset);
        }
    }
}