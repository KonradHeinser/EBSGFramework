using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class ButcherProductExtension : DefModExtension
    {
        public List<ThingDefCountClass> things;

        public bool useEfficiency = false;

        public ThingDef meatReplacement;

        public float meatAmountFactor = 1f;

        public ThingDef leatherReplacement;

        public float leatherAmountFactor = 1f;
    }
}
