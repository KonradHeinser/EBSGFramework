using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffWhileMoving : HediffComp
    {
        private HediffCompProperties_HediffWhileMoving Props => (HediffCompProperties_HediffWhileMoving)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.pather.Moving)
            {
                if (Props.hediffWhileMoving != null && !EBSGUtilities.HasHediff(Pawn, Props.hediffWhileMoving)) Pawn.health.AddHediff(Props.hediffWhileMoving);
                if (!Props.hediffsWhileMoving.NullOrEmpty())
                {
                    foreach (HediffDef hediff in Props.hediffsWhileMoving) if (!EBSGUtilities.HasHediff(Pawn, hediff)) Pawn.health.AddHediff(hediff);
                }
                if (Props.hediffWhileNotMoving != null) EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileNotMoving);
                if (!Props.hediffsWhileNotMoving.NullOrEmpty())
                {
                    foreach (HediffDef hediff in Props.hediffsWhileNotMoving) EBSGUtilities.RemoveHediffs(Pawn, hediff);
                }
            }
            else
            {
                if (Props.hediffWhileNotMoving != null && !EBSGUtilities.HasHediff(Pawn, Props.hediffWhileNotMoving)) Pawn.health.AddHediff(Props.hediffWhileNotMoving);
                if (!Props.hediffsWhileNotMoving.NullOrEmpty())
                {
                    foreach (HediffDef hediff in Props.hediffsWhileNotMoving) if (!EBSGUtilities.HasHediff(Pawn, hediff)) Pawn.health.AddHediff(hediff);
                }
                if (Props.hediffWhileMoving != null) EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileMoving);
                if (!Props.hediffsWhileMoving.NullOrEmpty())
                {
                    foreach (HediffDef hediff in Props.hediffsWhileMoving) EBSGUtilities.RemoveHediffs(Pawn, hediff);
                }
            }
        }
    }
}
