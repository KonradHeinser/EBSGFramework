namespace EBSGFramework
{
    public class HediffComp_SeverityWhenDowned : HediffComp_SetterBase
    {
        private HediffCompProperties_SeverityWhenDowned Props => (HediffCompProperties_SeverityWhenDowned)props;

        protected override void SetSeverity()
        {
            base.SetSeverity();
            
            if (Pawn.Downed)
                parent.Severity = Props.severity;
            else if (Props.nonDownedSeverity.HasValue)
                parent.Severity = Props.nonDownedSeverity.Value;
            
            ticksToNextCheck = 30;
        }
    }
}