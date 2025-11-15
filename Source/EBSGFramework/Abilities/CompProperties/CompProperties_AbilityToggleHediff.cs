using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityToggleHediff : CompProperties_AbilityEffect
    {
        public HediffToParts toggle;

        public CompProperties_AbilityToggleHediff()
        {
            compClass = typeof(CompAbilityEffect_ToggleHediff);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;
            if (toggle == null)
                yield return "toggle is doesn't have information listed.";
        }
    }
}