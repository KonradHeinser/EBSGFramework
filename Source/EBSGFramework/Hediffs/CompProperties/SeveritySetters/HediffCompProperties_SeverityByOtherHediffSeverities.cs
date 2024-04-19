using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByOtherHediffSeverities : HediffCompProperties
    {
        public List<HediffSeverityFactor> hediffSets;

        public float baseSeverity = 0.1f; // Severity assigned at the start. The hediff's severity doesn't drop below this unless one of the hediffSets has a negative factor

        public HediffCompProperties_SeverityByOtherHediffSeverities()
        {
            compClass = typeof(HediffComp_SeverityByOtherHediffSeverities);
        }
    }
}
