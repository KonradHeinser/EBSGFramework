using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_UseEffectAlterXenotype : CompProperties_UseEffect
    {
        public List<RandomXenotype> xenotypes;

        public FloatRange severities = new FloatRange(0f, 999f);

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // Clears old xeno genes and replaces them with the xenotype's

        public bool sendMessage = true;

        public Prerequisites prerequisites;

        public CompProperties_UseEffectAlterXenotype() { 
            compClass = typeof(CompUseEffect_AlterXenotype);
        }
    }
}
