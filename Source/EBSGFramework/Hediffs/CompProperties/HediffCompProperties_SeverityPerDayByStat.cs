using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityPerDayByStat : HediffCompProperties_SeverityPerDay
    {
        public StatDef stat;

        public FloatRange limits = new FloatRange(float.MinValue, float.MaxValue);
        
        public HediffCompProperties_SeverityPerDayByStat()
        {
            compClass = typeof(HediffComp_SeverityPerDayByStat);
        }
    }
}
