using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffsOnRemoval : HediffCompProperties
    {
        public List<HediffToGive> hediffsToGive;

        public FloatRange validSeverity = FloatRange.Zero;

        public HediffCompProperties_HediffsOnRemoval()
        {
            compClass = typeof(HediffComp_HediffsOnRemoval);
        }
    }
}
