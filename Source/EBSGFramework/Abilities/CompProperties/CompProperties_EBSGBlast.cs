using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_EBSGBlast : CompProperties_AbilityEffect
    {
        public ExplosionData explosion = new ExplosionData();

        public CompProperties_EBSGBlast()
        {
            compClass = typeof(CompAbilityEffect_EBSGBlast);
        }
    }
}
