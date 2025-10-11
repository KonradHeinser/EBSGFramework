using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityResurrectNoSideEffects : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityResurrectNoSideEffects()
        {
            compClass = typeof(CompAbilityEffect_ResurrectNoSideEffects);
        }
    }
}
