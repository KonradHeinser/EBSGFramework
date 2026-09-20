using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityPerDayByStat : HediffComp_SeverityPerDay
    {
        public HediffCompProperties_SeverityPerDayByStat Props => props as HediffCompProperties_SeverityPerDayByStat;
        
        public override float SeverityChangePerDay()
        {
            return Props.limits.ClampToRange(base.SeverityChangePerDay() * Pawn.StatOrOne(Props.stat));
        }
    }
}