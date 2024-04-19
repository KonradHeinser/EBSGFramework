using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByTimeOfDay : HediffCompProperties
    {
        public SimpleCurve timeToSeverityCurve;

        public HediffCompProperties_SeverityByTimeOfDay()
        {
            compClass = typeof(HediffComp_SeverityByTimeOfDay);
        }
    }
}
