using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityDestroyFilth : CompProperties_AbilityEffect
    {
        public IntRange amount = new IntRange(1,5);

        public List<ThingDef> validFilth;

        public List<ThingDef> invalidFilth;

        public CompProperties_AbilityDestroyFilth()
        {
            compClass = typeof(CompAbilityEffect_DestroyFilth);
        }
    }
}
