using System;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveMultipleHediffs : CompProperties_AbilityEffectWithDuration
    {
        public List<HediffToGive> hediffsToGive; // Each HediffToGive has the same contents as CompAbilityEffect_GiveHediff

        public CompProperties_AbilityGiveMultipleHediffs()
        {
            compClass = typeof(CompAbilityEffect_GiveMultipleHediffs);
        }
    }
}
