using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityBySeason : HediffComp_SetterBase
    {
        private HediffCompProperties_SeverityBySeason Props => (HediffCompProperties_SeverityBySeason)props;

        public override void Notify_Spawned()
        {
            base.Notify_Spawned();
            SetSeverity();
        }

        protected override void SetSeverity()
        {
            base.SetSeverity();

            ticksToNextCheck = 2500;
            var l = Props.seasonEffects.FirstOrDefault(s => Pawn.CheckSeason(s.seasons));
            if (l != null)
            {
                if (l.dayToSeverity != null)
                {
                    var severity = l.dayToSeverity.Evaluate(GenLocalDate.DayOfQuadrum(Pawn) + 1);
                    if (severity >= 0)
                        parent.Severity = severity;
                }
                else if (l.severity >= 0)
                    parent.Severity = l.severity;
            }
            else if (Props.defaultSeverity >= 0) 
                parent.Severity = Props.defaultSeverity;
        }
    }
}