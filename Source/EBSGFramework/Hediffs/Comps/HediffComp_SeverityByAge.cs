using Verse;
using RimWorld;
using System;

namespace EBSGFramework
{
    public class HediffComp_SeverityByAge : HediffComp
    {
        public HediffCompProperties_SeverityByAge Props => (HediffCompProperties_SeverityByAge)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            float severity = Pawn.ageTracker.AgeBiologicalYearsFloat;

            if (Pawn.RaceProps.Humanlike && Props.divideByLifespanFactor) severity /= Pawn.GetStatValue(StatDefOf.LifespanFactor);
            if (Props.accountForLifeExpectancyDifference && (!Pawn.RaceProps.IsMechanoid || Props.includeMechanoidLifeExpectancy)) severity *= 80 / Pawn.RaceProps.lifeExpectancy;
            severity *= Props.additionalFactor;

            parent.Severity = (float)Math.Round(severity, 2);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(2500)) return; // Even making it go every hour is probably too often, but this is just to be safe due to various age changing effects that exist

            float severity = Pawn.ageTracker.AgeBiologicalYearsFloat;

            if (Pawn.RaceProps.Humanlike && Props.divideByLifespanFactor) severity /= Pawn.GetStatValue(StatDefOf.LifespanFactor);
            if (Props.accountForLifeExpectancyDifference && (!Pawn.RaceProps.IsMechanoid || Props.includeMechanoidLifeExpectancy)) severity *= 80 / Pawn.RaceProps.lifeExpectancy;
            severity *= Props.additionalFactor;

            parent.Severity = (float)Math.Round(severity, 2);
        }
    }
}
