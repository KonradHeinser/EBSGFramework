using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityDestroyItem : CompProperties_AbilityEffect
    {
        public List<List<ThingLink>> options;

        public CompProperties_AbilityDestroyItem()
        {
            compClass = typeof(CompAbilityEffect_DestroyItem);
        }
    }
}
