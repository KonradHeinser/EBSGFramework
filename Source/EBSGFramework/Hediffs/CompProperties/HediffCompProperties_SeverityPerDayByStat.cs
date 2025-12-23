using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityPerDayByStat : HediffCompProperties_SeverityPerDay
    {
        public StatDef stat;

        public FloatRange limits;
        
        public HediffCompProperties_SeverityPerDayByStat()
        {
            compClass = typeof(HediffComp_SeverityPerDayByStat);
        }   
    }
}