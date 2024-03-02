using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityResourceToBattery : CompProperties_AbilityEffect
    {
        // Setting any max to 0 disables it

        public float maxConverted = 1f; // The maximum amount of the current resource value that can be used

        public float maxPercent = 1f; // The maximum percentage of the current resource value that can be used

        public float maxPercentOfMax = 1f; // The maximum percentage of the maximum resource value that can be used

        public float conversionEfficiency = 1f; // 0.1 resource (10 resource in game) lost becomes 10 energy. If set to 100, then that 0.1 would give 1,000 electricity

        public StatDef efficiencyFactorStat; // Additional multiplier on conversion efficiency

        public GeneDef mainResourceGene;

        public CompProperties_AbilityResourceToBattery()
        {
            compClass = typeof(CompAbilityEffect_ResourceToBattery);
        }
    }
}
