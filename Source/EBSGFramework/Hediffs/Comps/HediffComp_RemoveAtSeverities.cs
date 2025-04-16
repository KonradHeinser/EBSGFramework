using Verse;

namespace EBSGFramework
{
    public class HediffComp_RemoveAtSeverities : HediffComp
    {
        public HediffCompProperties_RemoveAtSeverities Props => (HediffCompProperties_RemoveAtSeverities)props;

        public override bool CompShouldRemove
        {
            get
            {
                return EBSGUtilities.WithinSeverityRanges(parent.Severity, Props.severity, Props.severities);
            }
        }
    }
}
