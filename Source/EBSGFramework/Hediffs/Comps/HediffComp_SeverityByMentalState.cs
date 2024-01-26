using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByMentalState : HediffComp
    {
        public HediffCompProperties_SeverityByMentalState Props => (HediffCompProperties_SeverityByMentalState)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.IsHashIntervalTick(100)) return;
            Pawn pawn = parent.pawn;

            if (pawn.InMentalState && !Props.mentalStateEffects.NullOrEmpty())
            {
                foreach (MentalStateEffect mentalStateEffect in Props.mentalStateEffects)
                {
                    if (mentalStateEffect.mentalState == null && mentalStateEffect.mentalStates.NullOrEmpty())
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                    if (mentalStateEffect.mentalState != null && pawn.MentalStateDef == mentalStateEffect.mentalState)
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                    if (!mentalStateEffect.mentalStates.NullOrEmpty() && mentalStateEffect.mentalStates.Contains(pawn.MentalStateDef))
                    {
                        parent.Severity = mentalStateEffect.mentalSeverity;
                        return;
                    }
                }
            }

            if (EBSGUtilities.GetCurrentTarget(pawn) != null) parent.Severity = Props.fightingSeverity;
            else if (pawn.Drafted) parent.Severity = Props.draftedSeverity;
            else parent.Severity = Props.defaultSeverity;
        }
    }
}
