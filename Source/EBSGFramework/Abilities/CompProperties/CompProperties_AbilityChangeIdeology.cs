using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityChangeIdeology : CompProperties_AbilityEffect
    {
        public FactionDef factionOfIdeo;

        public FloatRange certainty = FloatRange.One;

        public SuccessChance successChance;

        public CompProperties_AbilityChangeIdeology()
        {
            compClass = typeof(CompAbilityEffect_ChangeIdeology);
        }
    }
}
