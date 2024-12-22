using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveAtSeverities : HediffComp
    {
        public HediffCompProperties_RemoveAtSeverities Props => (HediffCompProperties_RemoveAtSeverities)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.IsHashIntervalTick(60))
                if (EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.severity, Props.severities))
                    Pawn.health.RemoveHediff(parent);
        }
    }
}
