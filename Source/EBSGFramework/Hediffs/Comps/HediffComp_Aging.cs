using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_Aging : HediffComp
    {
        private HediffCompProperties_Aging Props => (HediffCompProperties_Aging)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(200)) return;
            if ((Props.ageChangePerHour > 0 || Pawn.ageTracker.AgeBiologicalYearsFloat > Props.lowestAgeWhenNegative) && Props.ageChangePerHour != 0)
            {
                Pawn.ageTracker.AgeBiologicalTicks += (int)Math.Floor(Props.ageChangePerHour * 3600000 * 0.08f);
            }
            if ((Props.ageChangePerDay > 0 || Pawn.ageTracker.AgeBiologicalYearsFloat > Props.lowestAgeWhenNegative) && Props.ageChangePerDay != 0)
            {
                Pawn.ageTracker.AgeBiologicalTicks += (int)Math.Floor(Props.ageChangePerDay * 3600000 * 0.08f);
            }
        }
    }
}
