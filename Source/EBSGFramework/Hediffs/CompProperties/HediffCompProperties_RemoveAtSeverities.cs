using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemoveAtSeverities : HediffCompProperties
    {
        public FloatRange? severity = null;

        public List<FloatRange> severities;

        public HediffCompProperties_RemoveAtSeverities()
        {
            compClass = typeof(HediffComp_RemoveAtSeverities);
        }
    }
}
