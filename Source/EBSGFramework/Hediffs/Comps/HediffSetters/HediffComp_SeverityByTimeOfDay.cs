using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByTimeOfDay : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByTimeOfDay Props => (HediffCompProperties_SeverityByTimeOfDay)props;

        protected override void SetSeverity()
        {
            base.SetSeverity();
            parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));
            ticksToNextCheck = 120;
        }
    }
}
