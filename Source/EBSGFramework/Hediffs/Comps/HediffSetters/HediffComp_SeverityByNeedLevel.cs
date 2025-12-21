namespace EBSGFramework
{
    public class HediffComp_SeverityByNeedLevel : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByNeedLevel Props => (HediffCompProperties_SeverityByNeedLevel)props;

        protected override void SetSeverity()
        {
            var need = Pawn.needs.TryGetNeed(Props.need);
            if (need == null)
                parent.Severity = Props.severity.Evaluate(0f);
            else if (Props.usePercentage)
                parent.Severity = Props.severity.Evaluate(need.CurLevelPercentage);
            else
                parent.Severity = Props.severity.Evaluate(need.CurLevel);
            ticksToNextCheck = 120;
        }
    }
}