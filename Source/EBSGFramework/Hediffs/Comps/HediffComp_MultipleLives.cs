using RimWorld;
using Verse;
using System;

namespace EBSGFramework
{
    public class HediffComp_MultipleLives : HediffComp
    {
        public HediffCompProperties_MultipleLives Props => (HediffCompProperties_MultipleLives)props;

        public int livesLeft;

        public float progressPercentage;

        public bool pawnReviving;

        public float hoursToRevive;

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
                    string tooltipAddition = "\n";

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
                                tooltipAddition += "EBSG_TimeTillRevival".Translate(hoursToRevive *  (1 - revivalProgress), parent.pawn.Named("PAWN")).Resolve();
                            }
                        }
                        else
                        {
                            if (Props.useSeverityNotDays)
                            {
                                if (parent.Severity < parent.def.maxSeverity)
                                {
                                    float severityPerDay = 0;
                                    float severityRemaining = 1 - (severity % severityPerLife);

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
                                        tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(severityRemaining / severityPerDay, 1), parent.pawn.Named("PAWN")).Resolve();
                                    }
                                    else if (severityPerDay < 0)
                                    {
                                        tooltipAddition += "EBSG_TimeTillLifeLoss".Translate(Math.Round(severityRemaining / severityPerDay * -1, 1), parent.pawn.Named("PAWN")).Resolve();
                                    }
                                }
                            }
                            else if (progressPercentage > 0)
                            {
                                tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(Props.daysToRecoverLife * (1 - progressPercentage), 1), parent.pawn.Named("PAWN")).Resolve();
                            }
                        }
                        if (Props.includeRemainingLivesOnTooltip)
                        {
                            if (pawnReviving || (livesLeft < Props.extraLives && Props.extraLives != -666) || (Props.useSeverityNotDays && parent.Severity < parent.def.maxSeverity)) tooltipAddition += "\n\n";
                            if (Props.extraLives != -666)
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
                            else
                            {
                                tooltipAddition += "EBSG_Immortal".Translate(parent.pawn.Named("PAWN")).Resolve();
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
            livesLeft = Props.extraLives;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.pawn.IsHashIntervalTick(200))
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
                            livesLeft++;
                        }
                    }
                }
                else
                {
                    float maxSeverity = parent.def.maxSeverity - 0.001f; // only used for the severityNotDays
                    float severityPerLife = maxSeverity / Props.extraLives;
                    float severity = parent.Severity;
                    livesLeft = (int)Math.Floor(severity / severityPerLife);
                }
            }
        }

        public override void Notify_PawnDied()
        {
            if (Props.needBrainToRevive && Pawn.health.hediffSet.GetBrain() == null) return;
            if (Props.extraLives != -666)
            {
                if (Props.useSeverityNotDays)
                {
                    float maxSeverity = parent.def.maxSeverity - 0.001f; // To avoid reducing to 0 
                    float severityPerLife = maxSeverity / Props.extraLives;
                    float severity = parent.Severity;
                    if (severity - severityPerLife > 0)
                    {
                        parent.Severity -= severityPerLife;
                        livesLeft = (int)Math.Floor(severity / severityPerLife);
                    }
                    else return;
                }
                else
                {
                    livesLeft--;
                    if (livesLeft < 0) return;
                }
            }
            MultipleLives_Component multipleLives = Current.Game.GetComponent<MultipleLives_Component>();
            if (Props.hoursToRevive <= -1) hoursToRevive = -1;
            else if (Props.hoursToRevive > 0) hoursToRevive = Props.hoursToRevive;
            else hoursToRevive = Props.randomHoursToRevive.RandomInRange;

            if (multipleLives != null)
            {
                if (multipleLives.loaded)
                {
                    pawnReviving = true;
                    if (hoursToRevive <= -1) revivalProgress = 1;
                    multipleLives.AddPawnToLists(Pawn, parent.def, revivalProgress >= 1);
                }
                else
                {
                    Log.Error("The multiple lives game component failed to load. Please let the EBSG Framework dev know something went wrong!");
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref livesLeft, "EBSG_livesLeft", Props.extraLives);
            Scribe_Values.Look(ref progressPercentage, "EBSG_progressPercentage", 0);
            Scribe_Values.Look(ref pawnReviving, "EBSG_pawnReviving", false);
            Scribe_Values.Look(ref revivalProgress, "EBSG_revivalProgress", 0);
            Scribe_Values.Look(ref hoursToRevive, "EBSG_hoursToRevive", 0);
        }
    }
}
