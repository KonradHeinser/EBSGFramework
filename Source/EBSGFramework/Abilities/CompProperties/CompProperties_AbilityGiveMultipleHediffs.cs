using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveMultipleHediffs : CompProperties_AbilityEffectWithDuration
    {
        public List<HediffToGive> hediffsToGive; // Each HediffToGive has the same contents as CompAbilityEffect_GiveHediff

        public EndOn endOn = EndOn.End;

        public CompProperties_AbilityGiveMultipleHediffs()
        {
            compClass = typeof(CompAbilityEffect_GiveMultipleHediffs);
        }
    }
}
