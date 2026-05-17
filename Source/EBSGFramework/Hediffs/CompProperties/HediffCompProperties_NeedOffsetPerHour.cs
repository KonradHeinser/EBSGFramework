using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_NeedOffsetPerHour: HediffCompProperties
    {
        public List<NeedOffsetThreshold> needOffsets;

        public bool multiplyBySeverity = false;

        public HediffCompProperties_NeedOffsetPerHour()
        {
            compClass = typeof(HediffComp_NeedOffsetPerHour);
        }
    }
}