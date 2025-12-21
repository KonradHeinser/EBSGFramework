using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByNeedLevel : HediffCompProperties
    {
        public NeedDef need;

        public bool usePercentage = false;
        
        public SimpleCurve severity;

        public HediffCompProperties_SeverityByNeedLevel()
        {
            compClass = typeof(HediffComp_SeverityByNeedLevel);
        }
    }
}