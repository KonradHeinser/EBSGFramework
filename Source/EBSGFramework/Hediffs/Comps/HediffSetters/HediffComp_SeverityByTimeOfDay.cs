using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByTimeOfDay : HediffComp
    {
        public HediffCompProperties_SeverityByTimeOfDay Props => (HediffCompProperties_SeverityByTimeOfDay)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.timeToSeverityCurve != null)
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
            else
                parent.AddedHediffError(Pawn);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(200))
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
        }
    }
}
