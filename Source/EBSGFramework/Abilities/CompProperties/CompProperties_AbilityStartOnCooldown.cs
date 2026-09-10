using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityStartOnCooldown : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityStartOnCooldown()
        {
            compClass = typeof(CompAbilityEffect_StartOnCooldown);
        }
    }
}