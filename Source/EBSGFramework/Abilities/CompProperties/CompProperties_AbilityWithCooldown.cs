using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityWithCooldown : CompProperties_AbilityEffect
    {
        public int cooldownTicks = 10;

        public CompProperties_AbilityWithCooldown()
        {
            compClass = typeof(CompAbilityEffect_WithCooldown);
        }
    }
}
