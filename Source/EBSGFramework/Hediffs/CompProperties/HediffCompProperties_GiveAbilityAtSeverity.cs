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
    }
}
