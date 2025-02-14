using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityInterruptOnDamaged : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityInterruptOnDamaged()
        {
            compClass = typeof(CompAbilityEffect_InterruptOnDamaged);
        }
    }
}
