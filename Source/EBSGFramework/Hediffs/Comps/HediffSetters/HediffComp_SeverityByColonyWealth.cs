namespace EBSGFramework
{
    public class HediffComp_SeverityByColonyWealth : HediffComp_SetterBase
    {
        private HediffCompProperties_SeverityByColonyWealth Props => (HediffCompProperties_SeverityByColonyWealth)props;
        
        protected override bool MustBeSpawned => true;
        
        protected override void SetSeverity()
        {
            parent.Severity = Props.curve?.Evaluate(Pawn?.MapHeld?.wealthWatcher?.WealthTotal ?? 0) ?? 0.0001f; // Null checks are probably not needed, but better safe than erroring
            ticksToNextCheck = 2500;
        }
    }
}
