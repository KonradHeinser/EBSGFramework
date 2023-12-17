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
                EBSGUtilities.AddOrAppendHediffs(Pawn, Props.initialMovingSeverity, Props.movingSeverityPerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
            }
            else
            {
                EBSGUtilities.AddOrAppendHediffs(Pawn, Props.initialNotMovingSeverity, Props.notMovingSeverityPerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileMoving, Props.hediffsWhileMoving);
            }
        }
    }
}
