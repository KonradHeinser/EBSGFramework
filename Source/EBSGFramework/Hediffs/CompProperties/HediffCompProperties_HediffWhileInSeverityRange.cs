using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffWhileInSeverityRange : HediffCompProperties
    {
        public List<HediffSeverityLevel> hediffsAtSeverities;

        public HediffCompProperties_HediffWhileInSeverityRange()
        {
            compClass = typeof(HediffComp_HediffWhileInSeverityRange);
        }
    }
}
