using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityAlterXenotype : CompProperties_AbilityEffect
    {
        public bool useCasterXeno = false;

        public List<RandomXenotype> xenotypes;

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // Clears old xeno genes and replaces them with the xenotype's

        public bool sendMessage = true;

        public CompProperties_AbilityAlterXenotype()
        {
            compClass = typeof(CompAbilityEffect_AlterXenotype);
        }
    }
}
