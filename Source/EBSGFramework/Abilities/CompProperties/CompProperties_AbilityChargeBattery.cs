using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityChargeBattery : CompProperties_AbilityEffect
    {
        public int batteryChangeAmount = 0;

        public StatDef efficiencyFactorStat; // Stat multiplier on batteryChangeAmount

        public bool allowOvercharge = true; // While false, if the change amount is positive it will be limited by the maximum stored energy

        public CompProperties_AbilityChargeBattery()
        {
            compClass = typeof(CompAbilityEffect_ChargeBattery);
        }
    }
}
