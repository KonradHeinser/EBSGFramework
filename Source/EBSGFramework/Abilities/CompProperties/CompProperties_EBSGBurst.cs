using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_EBSGBurst : CompProperties_AbilityEffect
    {
        public ExplosionData explosion;
        
        public SuccessChance successChance;

        public CompProperties_EBSGBurst()
        {
            compClass = typeof(CompAbilityEffect_EBSGBurst);
        }
    }
}
