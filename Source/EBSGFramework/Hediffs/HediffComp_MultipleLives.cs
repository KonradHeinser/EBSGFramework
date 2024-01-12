using RimWorld;
using Verse;
using System;

namespace EBSGFramework
{
    public class HediffComp_MultipleLives : HediffComp
    {
        private HediffCompProperties_MultipleLives Props => (HediffCompProperties_MultipleLives)props;

        public int livesLeft;

        public float progressPercentage;

        public bool pawnReviving;

        public float revivalProgress;

        public override string CompTipStringExtra
        {
            get
            {
                if (Props.includeProgressOnTooltip || Props.includeRemainingLivesOnTooltip)
                {
                    float maxSeverity = parent.def.maxSeverity - 0.001f; // only used for the severityNotDays
                    float severityPerLife = maxSeverity / Props.extraLives;
                    float severity = parent.Severity;
                    string tooltipAddition = "";

                    if (Props.includeProgressOnTooltip)
                    {
                        if (pawnReviving)
                        {
                            if (revivalProgress >= 1)
                            {
                                tooltipAddition += "EBSG_RevivalImminent".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else
                            {
                                tooltipAddition += "EBSG_TimeTillRevival".Translate(Props.hoursToRevive / (1 - revivalProgress), parent.pawn.Named("PAWN")).Resolve();
                            }

                        }
                        else
                        {
                            if (Props.useSeverityNotDays)
                            {
                                if (parent.Severity < parent.def.maxSeverity)
                                {
                                    float severityPerDay = 0;
                                    foreach (HediffCompProperties compProps in parent.def.comps)
                                    {
                                        if (compProps is HediffCompProperties_SeverityPerDay severityComp)
                                        {
                                            if (severityComp.severityPerDay > 0) severityPerDay = severityComp.severityPerDay;
                                            else severityPerDay = severityComp.severityPerDayRange.Average;
                                            break;
                                        }
                                    }
                                    if (severityPerDay > 0)
                                    {
                                        tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(severity % severityPerLife / severityPerDay, 1), parent.pawn.Named("PAWN")).Resolve();
                                    }
                                    else if (severityPerDay < 0)
                                    {
                                        tooltipAddition += "EBSG_TimeTillLifeLoss".Translate(Math.Round(severity % severityPerLife / severityPerDay * -1, 1), parent.pawn.Named("PAWN")).Resolve();
                                    }
                                }
                            }
                            else
                            {
                                tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(Props.daysToRecoverLife * (1 - progressPercentage), 1), parent.pawn.Named("PAWN")).Resolve();
                            }
                        }
                        if (Props.includeRemainingLivesOnTooltip && Props.extraLives != -666)
                        {
                            if (pawnReviving || livesLeft < Props.extraLives) tooltipAddition += "\n\n";
                            if (Props.useSeverityNotDays)
                            {

                                int livesRemaining = (int)Math.Floor(severity / severityPerLife);
                                if (livesRemaining == 0)
                                {
                                    tooltipAddition += "EBSG_LastLife".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else if (livesRemaining == 1 && Props.extraLives == 1)
                                {
                                    tooltipAddition += "EBSG_OneLifeStill".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else if (livesRemaining == 1)
                                {
                                    tooltipAddition += "EBSG_OneLifeLeft".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else
                                {
                                    tooltipAddition += "EBSG_LivesLeft".Translate(livesRemaining, parent.pawn.Named("PAWN")).Resolve();
                                }
                            }
                            else
                            {
                                if (livesLeft == 0)
                                {
                                    tooltipAddition += "EBSG_LastLife".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else if (livesLeft == 1 && Props.extraLives == 1)
                                {
                                    tooltipAddition += "EBSG_OneLifeStill".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else if (livesLeft == 1)
                                {
                                    tooltipAddition += "EBSG_OneLifeLeft".Translate(parent.pawn.Named("PAWN")).Resolve();
                                }
                                else
                                {
                                    tooltipAddition += "EBSG_LivesLeft".Translate(livesLeft, parent.pawn.Named("PAWN")).Resolve();
                                }
                            }
                        }
                    }
                    else if (Props.extraLives != -666)
                    {
                        if (Props.useSeverityNotDays)
                        {
                            int livesRemaining = (int)Math.Floor(severity / severityPerLife);
                            if (livesRemaining == 0)
                            {
                                tooltipAddition += "EBSG_LastLife".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else if (livesRemaining == 1 && Props.extraLives == 1)
                            {
                                tooltipAddition += "EBSG_OneLifeStill".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else if (livesRemaining == 1)
                            {
                                tooltipAddition += "EBSG_OneLifeLeft".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else
                            {
                                tooltipAddition += "EBSG_LivesLeft".Translate(livesRemaining, parent.pawn.Named("PAWN")).Resolve();
                            }
                        }
                        else
                        {
                            if (livesLeft == 0)
                            {
                                tooltipAddition += "EBSG_LastLife".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else if (livesLeft == 1 && Props.extraLives == 1)
                            {
                                tooltipAddition += "EBSG_OneLifeStill".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else if (livesLeft == 1)
                            {
                                tooltipAddition += "EBSG_OneLifeLeft".Translate(parent.pawn.Named("PAWN")).Resolve();
                            }
                            else
                            {
                                tooltipAddition += "EBSG_LivesLeft".Translate(livesLeft, parent.pawn.Named("PAWN")).Resolve();
                            }
                        }
                    }
                    else return "";
                    return tooltipAddition;
                }
                return "";
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Log.Message("Tick");
            if (parent.pawn.IsHashIntervalTick(200))
            {
                if (pawnReviving)
                {
                    if (!parent.pawn.Dead) // If revived through some other means, then stop trying to revive
                    {
                        revivalProgress = 0;
                        pawnReviving = false;
                        return;
                    }
                    float revivalSpeed = 0;
                    if (Props.hoursToRevive > 0) revivalSpeed = 1 / Props.hoursToRevive * 0.08f; // For example, a 24 hour revival adds 0.0033333% progress every viable iteration, which occurs 300 times per day
                    revivalProgress += revivalSpeed;
                    if (revivalProgress >= 1)
                    {
                        revivalProgress = 0;
                        RevivePawn();
                    }
                }
                else
                {
                    if (!Props.useSeverityNotDays)
                    {
                        if (livesLeft > Props.extraLives) livesLeft = Props.extraLives;
                        else if (livesLeft < Props.extraLives && Props.daysToRecoverLife > 0)
                        {
                            float recoverySpeed = 1 / Props.daysToRecoverLife * 0.003333334f; // For example, a 20 day recover speed creates a progress of 0.000166667% each viable iteration, which happens 300 times per day
                            progressPercentage += recoverySpeed;
                            if (progressPercentage >= 1)
                            {
                                progressPercentage -= 1;
                                livesLeft += 1;
                            }
                        }
                    }
                }
            }
        }

        public override void Notify_PawnDied()
        {
            if (Props.extraLives == -666)
            {
                if (Props.hoursToRevive > 0) pawnReviving = true;
                else RevivePawn();
            }
            else if (Props.useSeverityNotDays)
            {
                float maxSeverity = parent.def.maxSeverity - 0.001f; // To avoid reducing to 0 
                float severityPerLife = maxSeverity / Props.extraLives;
                float severity = parent.Severity;
                if (severity - severityPerLife > 0)
                {
                    parent.Severity -= severityPerLife;
                    if (Props.hoursToRevive > 0) pawnReviving = true;
                    else ResurrectionUtility.Resurrect(parent.pawn.Corpse.InnerPawn);
                }
            }
            else
            {
                livesLeft--;
                if (livesLeft < 0) return;
                Log.Message("Revivng");
                if (Props.hoursToRevive > 0) pawnReviving = true;
                else ResurrectionUtility.Resurrect(parent.pawn.Corpse.InnerPawn);
                Log.Message("Revive done");
            }
        }

        private void RevivePawn()
        {
            pawnReviving = false;
            if (parent.pawn.Dead && !parent.pawn.Discarded) ResurrectionUtility.Resurrect(parent.pawn.Corpse.InnerPawn);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref livesLeft, "EBSG_livesLeft", Props.extraLives);
            Scribe_Values.Look(ref progressPercentage, "EBSG_progressPercentage", 0);
            Scribe_Values.Look(ref pawnReviving, "EBSG_pawnReviving", false);
            Scribe_Values.Look(ref revivalProgress, "EBSG_revivalProgress", 0);
        }
    }
}
