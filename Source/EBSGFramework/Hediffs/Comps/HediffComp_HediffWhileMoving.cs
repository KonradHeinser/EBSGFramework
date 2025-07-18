﻿using Verse;

namespace EBSGFramework
{
    public class HediffComp_HediffWhileMoving : HediffComp
    {
        private HediffCompProperties_HediffWhileMoving Props => (HediffCompProperties_HediffWhileMoving)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            if (Pawn.pather.Moving)
            {
                Pawn.AddOrAppendHediffs(Props.initialMovingSeverity, Props.movingSeverityPerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                if (Props.fadingHediffs || Props.fadingNotMovingHediff)
                {
                    if (Props.severityFadePerHour > 0) Pawn.FadingHediffs(Props.severityFadePerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else if (Props.notMovingSeverityFadePerHour > 0) Pawn.FadingHediffs(Props.notMovingSeverityFadePerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else if (Props.notMovingSeverityPerHour > 0) Pawn.FadingHediffs(Props.notMovingSeverityPerHour / 2500 * -1, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                    else Pawn.RemoveHediffs(Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                }
                else Pawn.RemoveHediffs(Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
            }
            else
            {
                Pawn.AddOrAppendHediffs(Props.initialNotMovingSeverity, Props.notMovingSeverityPerHour / 2500, Props.hediffWhileNotMoving, Props.hediffsWhileNotMoving);
                if (Props.fadingHediffs || Props.fadingMovingHediff)
                {
                    if (Props.severityFadePerHour > 0) Pawn.FadingHediffs(Props.severityFadePerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else if (Props.movingSeverityFadePerHour > 0) Pawn.FadingHediffs(Props.movingSeverityFadePerHour / 2500, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else if (Props.movingSeverityPerHour > 0) Pawn.FadingHediffs(Props.movingSeverityPerHour / 2500 * -1, Props.hediffWhileMoving, Props.hediffsWhileMoving);
                    else Pawn.RemoveHediffs(Props.hediffWhileMoving, Props.hediffsWhileMoving);
                }
                else Pawn.RemoveHediffs(Props.hediffWhileMoving, Props.hediffsWhileMoving);
            }
        }
    }
}
