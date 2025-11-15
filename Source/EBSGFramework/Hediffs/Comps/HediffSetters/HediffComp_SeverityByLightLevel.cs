using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_SeverityByLightLevel : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByLightLevel Props => (HediffCompProperties_SeverityByLightLevel)props;

        protected override bool MustBeSpawned => true;

        protected override void SetSeverity()
        {
            if (Pawn.Map != null)
                parent.Severity = Props.lightToSeverityCurve.Evaluate(Pawn.Map.glowGrid.GroundGlowAt(Pawn.Position));
            else if (Props.timeToSeverityCurve != null)
                parent.Severity = Props.timeToSeverityCurve.Evaluate(GenLocalDate.DayPercent(Pawn));

            ticksToNextCheck = 60;
        }
    }
}
