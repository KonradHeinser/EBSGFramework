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
            {
                var state = Pawn.MentalStateDef;
                var effect = Props.mentalStateEffects.FirstOrDefault(e => (e.mentalState == null && e.mentalStates.NullOrEmpty()) ||
                                                    (e.mentalState != null && state == e.mentalState) ||
                                                    (!e.mentalStates.NullOrEmpty() && e.mentalStates.Contains(state)));
                if (effect != null)
                {
                    if (effect.addSeverityPerHour)
                        parent.Severity += effect.mentalSeverity * ticksToNextCheck / 2500f;
                    else
                        parent.Severity = effect.mentalSeverity;
                    return;
                }
            }

            if (Pawn.GetCurrentTarget() != null)
            {
                if (Props.addSeverityPerHour)
                    parent.Severity += Props.fightingSeverity * ticksToNextCheck / 2500f;
                else if (Props.fightingSeverity >= 0)
                    parent.Severity = Props.fightingSeverity;
            }
            else if (Pawn.Drafted)
            {
                if (Props.addSeverityPerHour)
                    parent.Severity += Props.draftedSeverity * ticksToNextCheck / 2500f;
                else if (Props.draftedSeverity >= 0)
                    parent.Severity = Props.draftedSeverity;
            }
            else if (Props.addSeverityPerHour)
                parent.Severity += Props.defaultSeverity * ticksToNextCheck / 2500f;
            else if (Props.defaultSeverity >= 0)
                parent.Severity = Props.defaultSeverity;
        }
    }
}
