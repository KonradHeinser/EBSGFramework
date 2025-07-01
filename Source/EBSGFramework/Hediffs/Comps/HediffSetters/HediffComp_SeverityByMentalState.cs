using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByMentalState : HediffComp_SetterBase
    {
        public HediffCompProperties_SeverityByMentalState Props => (HediffCompProperties_SeverityByMentalState)props;

        protected override void SetSeverity()
        {
            base.SetSeverity();
            ticksToNextCheck = 120;
            if (Pawn.InMentalState && !Props.mentalStateEffects.NullOrEmpty())
                foreach (MentalStateEffect mentalStateEffect in Props.mentalStateEffects)
                {
                    if (mentalStateEffect.mentalState == null && mentalStateEffect.mentalStates.NullOrEmpty())
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                    if (mentalStateEffect.mentalState != null && Pawn.MentalStateDef == mentalStateEffect.mentalState)
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                    if (!mentalStateEffect.mentalStates.NullOrEmpty() && mentalStateEffect.mentalStates.Contains(Pawn.MentalStateDef))
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                }

            if (Pawn.GetCurrentTarget() != null) 
                parent.Severity = Props.fightingSeverity;
            else if (Pawn.Drafted) 
                parent.Severity = Props.draftedSeverity;
            else 
                parent.Severity = Props.defaultSeverity;
        }
    }
}
