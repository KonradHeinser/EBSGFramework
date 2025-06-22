using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByTimeOfDay : HediffComp
    {
        public HediffCompProperties_SeverityByTimeOfDay Props => (HediffCompProperties_SeverityByTimeOfDay)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
        }
    }
}
