using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_CapacityFactor : HediffCompProperties
    {
        public List<CapFactor> capMods;

        public FloatRange validSeverity = FloatRange.Zero;

        public HediffCompProperties_CapacityFactor() 
        {
            compClass = typeof(HediffComp_CapacityFactor);
        }
    }
}
