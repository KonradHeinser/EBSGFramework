using System;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByAge : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByAge Props => (HediffCompProperties_SeverityByAge)props;

        protected override void SetSeverity()
        {
            float severity = Pawn.ageTracker.AgeBiologicalYearsFloat;

            if (Pawn.RaceProps.Humanlike && Props.divideByLifespanFactor) severity /= Pawn.GetStatValue(StatDefOf.LifespanFactor);
            if (Props.accountForLifeExpectancyDifference && (!Pawn.RaceProps.IsMechanoid || Props.includeMechanoidLifeExpectancy)) severity *= 80 / Pawn.RaceProps.lifeExpectancy;
            severity *= Props.additionalFactor;

            parent.Severity = (float)Math.Round(severity, 2);
            ticksToNextCheck = 2500;
        }
    }
}
