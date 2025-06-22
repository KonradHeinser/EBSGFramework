using System;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_Aging : HediffComp
    {
        private HediffCompProperties_Aging Props => (HediffCompProperties_Aging)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if ((Props.ageChangePerHour > 0 || Pawn.ageTracker.AgeBiologicalYearsFloat > Props.lowestAgeWhenNegative) && Props.ageChangePerHour != 0)
                Pawn.ageTracker.AgeBiologicalTicks += (int)Math.Floor(Props.ageChangePerHour * 1440f * delta);
            if ((Props.ageChangePerDay > 0 || Pawn.ageTracker.AgeBiologicalYearsFloat > Props.lowestAgeWhenNegative) && Props.ageChangePerDay != 0)
                Pawn.ageTracker.AgeBiologicalTicks += (int)Math.Floor(Props.ageChangePerDay * 60f * delta);
        }
    }
}
