using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_LoveTheCaster : CompProperties_AbilityEffect
    {
        public HediffDef hediffToApply;

        public CompProperties_LoveTheCaster()
        {
            compClass = typeof(CompAbilityEffect_LoveTheCaster);
        }
    }
}
