using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveAtSeverities : HediffComp
    {
        public HediffCompProperties_RemoveAtSeverities Props => (HediffCompProperties_RemoveAtSeverities)props;

        public override bool CompShouldRemove => EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.severity, Props.severities);

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            
            if (!Props.addedHediffsOnRemove.NullOrEmpty() && EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.severity, Props.severities))
                Pawn.AddHediffToParts(Props.addedHediffsOnRemove);
        }
    }
}
