﻿using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByLightLevel : HediffComp
    {
        public HediffCompProperties_SeverityByLightLevel Props => (HediffCompProperties_SeverityByLightLevel)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.lightToSeverityCurve == null)
                EBSGUtilities.AddedHediffError(parent, Pawn);

            if (Pawn.Map != null)
                parent.Severity = Props.lightToSeverityCurve.Evaluate(Pawn.Map.glowGrid.GameGlowAt(Pawn.Position, false));
            else if (Props.timeToSeverityCurve != null)
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!Pawn.IsHashIntervalTick(200)) return;

            if (Pawn.Map != null)
                parent.Severity = Props.lightToSeverityCurve.Evaluate(Pawn.Map.glowGrid.GameGlowAt(Pawn.Position, false));
            else if (Props.timeToSeverityCurve != null)
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
        }
    }
}
