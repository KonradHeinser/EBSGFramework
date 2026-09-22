using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveHediffUntilLimit : CompProperties_AbilityEffect
    {
        public List<HediffUntilLimit> hediffs = new List<HediffUntilLimit>();
        
        public CompProperties_AbilityGiveHediffUntilLimit()
        {
            compClass = typeof(CompAbilityEffect_GiveHediffUntilLimit);
        }
    }
}
