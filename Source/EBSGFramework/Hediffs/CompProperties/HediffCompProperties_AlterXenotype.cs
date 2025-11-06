using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_AlterXenotype : HediffCompProperties
    {
        public List<RandomXenotype> xenotypes;

        public FloatRange severities = new FloatRange(0f, 999f);

        public ThingDef filth;

        public IntRange filthCount = new IntRange(4, 7);

        public bool setXenotype = true; // Clears old xeno genes and replaces them with the xenotype's

        public bool sendMessage = true;

        public HediffCompProperties_AlterXenotype()
        {
            compClass = typeof(HediffComp_AlterXenotype);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (xenotypes.NullOrEmpty())
                yield return "xenotypes needs to contain something.";
        }
    }
}
