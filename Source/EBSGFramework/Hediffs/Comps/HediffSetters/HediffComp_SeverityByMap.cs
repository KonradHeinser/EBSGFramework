namespace EBSGFramework
{
    public class HediffComp_SeverityByMap : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByMap Props => (HediffCompProperties_SeverityByMap)props;

        private bool prevMapCheck;

        protected override bool DoCheck() => prevMapCheck != (Pawn.MapHeld != null);

        protected override void SetSeverity()
        {
            base.SetSeverity();
            ticksToNextCheck = 600;
            var map = Pawn.MapHeld;
            // If we're already using world map severity, no need to do anything else
            if (map == null)
            {
                if (!prevMapCheck)
                    return;
                prevMapCheck = false;
                parent.Severity = Props.notInMap;
            }
            else
            {
                prevMapCheck = true;
                if (Props.inTempIncidentMap != 0 && map.IsTempIncidentMap)
                    parent.Severity = Props.inTempIncidentMap; // Same, but for things like ambushes or camps
                else if (Props.inPlayerHome != 0 && map.IsPlayerHome) // If colony maps have a different severity, check for that
                    parent.Severity = Props.inPlayerHome;
                else if (Props.inPocketMap != 0 && map.IsPocketMap) // Do the same, but for pocket maps
                    parent.Severity = Props.inPocketMap;
                else
                    parent.Severity = Props.inMap;
            }
        }
    }
}