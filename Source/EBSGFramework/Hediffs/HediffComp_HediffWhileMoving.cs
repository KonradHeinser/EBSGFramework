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
                if (Props.fadingHediffs || Props.fadingNotMovingHediff)
                {
                    if (Props.severityFadePerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.severityFadePerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else if (Props.notMovingSeverityFadePerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.notMovingSeverityFadePerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else if (Props.notMovingSeverityPerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.notMovingSeverityPerHour / 2500 * -1, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                }
                else EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
            }
            else
            {
                EBSGUtilities.AddOrAppendHediffs(Pawn, Props.initialNotMovingSeverity, Props.notMovingSeverityPerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                if (Props.fadingHediffs || Props.fadingMovingHediff)
                {
                    if (Props.severityFadePerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.severityFadePerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else if (Props.movingSeverityFadePerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.movingSeverityFadePerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else if (Props.movingSeverityPerHour > 0) EBSGUtilities.FadingHediffs(Pawn, Props.movingSeverityPerHour / 2500 * -1, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                }
                else EBSGUtilities.RemoveHediffs(Pawn, Props.hediffWhileMoving, Props.hediffsWhileMoving);
            }
        }
    }
}
