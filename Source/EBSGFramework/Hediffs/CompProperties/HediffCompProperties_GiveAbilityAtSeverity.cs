using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GiveAbilityAtSeverity : HediffCompProperties
    {
        public List<AbilitiesAtSeverities> abilitiesAtSeverities;

        public HediffCompProperties_GiveAbilityAtSeverity()
        {
            compClass = typeof(HediffComp_GiveAbilityAtSeverity);
        }
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (abilitiesAtSeverities.NullOrEmpty())
                yield return "abilitiesAtSeverities needs to be set.";
        }
    }
}
