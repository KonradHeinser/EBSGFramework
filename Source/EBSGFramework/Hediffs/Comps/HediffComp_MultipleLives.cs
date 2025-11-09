using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_MultipleLives : HediffComp
    {
        public HediffCompProperties_MultipleLives Props => (HediffCompProperties_MultipleLives)props;

        public int livesLeft;

        public int deathTile;

        public float progressPercentage;

        public bool pawnReviving;

        public float hoursToRevive;

        public float revivalProgress;

        public override bool CompShouldRemove
        {
            get
            {
                if (livesLeft == 0 && Props.deleteOnFinalRevive)
                    return true;
                return base.CompShouldRemove;
            }
        }

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
                                tooltipAddition += "EBSG_RevivalImminent".Translate(Pawn.Named("PAWN")).Resolve();
                            else
                                tooltipAddition += "EBSG_TimeTillRevival".Translate(hoursToRevive * (1 - revivalProgress), Pawn.Named("PAWN")).Resolve();
                            tooltipAddition += " ";
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
                                        if (compProps is HediffCompProperties_SeverityPerDay severityComp)
                                        {
                                            if (severityComp.severityPerDay > 0) severityPerDay = severityComp.severityPerDay;
                                            else severityPerDay = severityComp.severityPerDayRange.Average;
                                            break;
                                        }
                                    if (severityPerDay > 0)
                                        tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(severityRemaining / severityPerDay, 1), Pawn.Named("PAWN")).Resolve();
                                    else if (severityPerDay < 0)
                                        tooltipAddition += "EBSG_TimeTillLifeLoss".Translate(Math.Round(severityRemaining / severityPerDay * -1, 1), Pawn.Named("PAWN")).Resolve();
                                    tooltipAddition += " ";
                                }
                            }
                            else if (progressPercentage > 0)
                            {
                                tooltipAddition += "EBSG_TimeTillLifeGain".Translate(Math.Round(Props.daysToRecoverLife * (1 - progressPercentage), 1), Pawn.Named("PAWN")).Resolve();
                                tooltipAddition += " ";
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
                                        tooltipAddition += "EBSG_LastLife".Translate(Pawn.Named("PAWN")).Resolve();
                                    else if (livesRemaining == 1 && Props.extraLives == 1)
                                        tooltipAddition += "EBSG_OneLifeStill".Translate(Pawn.Named("PAWN")).Resolve();
                                    else if (livesRemaining == 1)
                                        tooltipAddition += "EBSG_OneLifeLeft".Translate(Pawn.Named("PAWN")).Resolve();
                                    else
                                        tooltipAddition += "EBSG_LivesLeft".Translate(livesRemaining, Pawn.Named("PAWN")).Resolve();
                                }
                                else
                                {
                                    if (livesLeft == 0)
                                        tooltipAddition += "EBSG_LastLife".Translate(Pawn.Named("PAWN")).Resolve();
                                    else if (livesLeft == 1 && Props.extraLives == 1)
                                        tooltipAddition += "EBSG_OneLifeStill".Translate(Pawn.Named("PAWN")).Resolve();
                                    else if (livesLeft == 1)
                                        tooltipAddition += "EBSG_OneLifeLeft".Translate(Pawn.Named("PAWN")).Resolve();
                                    else
                                        tooltipAddition += "EBSG_LivesLeft".Translate(livesLeft, Pawn.Named("PAWN")).Resolve();
                                }
                            }
                            else
                                tooltipAddition += "EBSG_Immortal".Translate(Pawn.Named("PAWN")).Resolve();
                            tooltipAddition += " ";
                        }
                    }
                    else if (Props.extraLives != -666)
                    {
                        if (Props.useSeverityNotDays)
                        {
                            int livesRemaining = (int)Math.Floor(severity / severityPerLife);
                            if (livesRemaining == 0)
                                tooltipAddition += "EBSG_LastLife".Translate(Pawn.Named("PAWN")).Resolve();
                            else if (livesRemaining == 1 && Props.extraLives == 1)
                                tooltipAddition += "EBSG_OneLifeStill".Translate(Pawn.Named("PAWN")).Resolve();
                            else if (livesRemaining == 1)
                                tooltipAddition += "EBSG_OneLifeLeft".Translate(Pawn.Named("PAWN")).Resolve();
                            else
                                tooltipAddition += "EBSG_LivesLeft".Translate(livesRemaining, Pawn.Named("PAWN")).Resolve();
                        }
                        else
                        {
                            if (livesLeft == 0)
                                tooltipAddition += "EBSG_LastLife".Translate(Pawn.Named("PAWN")).Resolve();
                            else if (livesLeft == 1 && Props.extraLives == 1)
                                tooltipAddition += "EBSG_OneLifeStill".Translate(Pawn.Named("PAWN")).Resolve();
                            else if (livesLeft == 1)
                                tooltipAddition += "EBSG_OneLifeLeft".Translate(Pawn.Named("PAWN")).Resolve();
                            else
                                tooltipAddition += "EBSG_LivesLeft".Translate(livesLeft, Pawn.Named("PAWN")).Resolve();
                        }
                        tooltipAddition += " ";
                    }

                    if (!pawnReviving)
                    {
                        if (Props.revivalChance < 1 && livesLeft != 0)
                        {
                            if (Props.onlyOneChance || livesLeft == 1 || Props.extraLives == -666)
                                tooltipAddition += "EBSG_ReviveFailOneChance".Translate(Props.revivalChance.ToStringPercent()).Resolve();
                            else
                                tooltipAddition += "EBSG_ReviveFailChance".Translate(Props.revivalChance.ToStringPercent()).Resolve();
                            tooltipAddition += " ";
                        }

                        if (Props.needBrainToRevive && livesLeft != 0)
                            tooltipAddition += "EBSG_FailReviveOnHeadRemoval".Translate(Pawn.Named("PAWN")).Resolve();
                    }

                    if (tooltipAddition == "\n") return "";
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
            if (Pawn.IsHashIntervalTick(200))
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

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (Props.needBrainToRevive && Pawn.health.hediffSet.GetBrain() == null) return;

            if (!Rand.Chance(Props.revivalChance))
            {
                if (Props.extraLives == -666 || Props.onlyOneChance)
                {
                    FlashyFail();
                    return;
                }
                if (Props.useSeverityNotDays)
                    while (true)
                    {
                        float maxSeverity = parent.def.maxSeverity - 0.001f; // To avoid reducing to 0 
                        float severityPerLife = maxSeverity / Props.extraLives;
                        float severity = parent.Severity;
                        if (severity - severityPerLife > 0)
                        {
                            parent.Severity -= severityPerLife;
                            livesLeft = (int)Math.Floor(severity / severityPerLife);
                        }
                        else
                        {
                            FlashyFail();
                            return;
                        }
                        if (Rand.Chance(Props.revivalChance)) break;
                    }
                else
                    while (true)
                    {
                        livesLeft--;
                        if (livesLeft < 0)
                        {
                            FlashyFail();
                            return;
                        }
                        if (Rand.Chance(Props.revivalChance)) break;
                    }
            }
            deathTile = Pawn.Tile;
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
                    else
                    {
                        FlashyFail();
                        return;
                    }
                }
                else
                {
                    livesLeft--;
                    if (livesLeft < 0)
                    {
                        FlashyFail();
                        return;
                    }
                }
            }

            MultipleLives_Component multipleLives = Current.Game.GetComponent<MultipleLives_Component>();
            if (Props.hoursToRevive <= -1) hoursToRevive = -1;
            else if (Props.hoursToRevive > 0) hoursToRevive = Props.hoursToRevive;
            else hoursToRevive = Props.randomHoursToRevive.RandomInRange;

            if (multipleLives != null && multipleLives.loaded)
            {
                pawnReviving = true;
                if (hoursToRevive <= -1) revivalProgress = 1;
                multipleLives.AddPawnToLists(Pawn, parent.def, revivalProgress >= 1, Props.indestructibleWhileResurrecting && Props.alwaysForbiddenWhileResurrecting);
                OnReviveStart();
            }
            else
                Log.Error("The multiple lives game component failed to load. Please let the EBSG Framework dev know something went wrong!");
        }

        private void OnReviveStart()
        {
            if (Pawn.Corpse == null || Pawn.Corpse.Destroyed) return;
            Map map = Pawn.MapHeld;

            EBSGUtilities.ThingAndSoundMaker(Pawn.Corpse.Position, map, Props.thingSpawnOnReviveStart, Props.thingsToSpawnOnReviveStart, Props.reviveStartSound);
        }

        private void FlashyFail()
        {
            if (Pawn.Faction.IsPlayer)
            {
                if (Props.revivalSuccessMessage != null)
                    Messages.Message(Props.revivalFailedMessage.TranslateOrFormat(Pawn.LabelShortCap, livesLeft.ToString()),
                        MessageTypeDefOf.NegativeHealthEvent);
                if (Props.revivalSuccessLetterLabel != null)
                {
                    Letter letter = LetterMaker.MakeLetter(Props.revivalFailedLetterLabel.TranslateOrFormat(Pawn.LabelShortCap, livesLeft.ToString()),
                        Props.revivalFailedLetterDescription.TranslateOrFormat(Pawn.LabelShortCap, livesLeft.ToString()),
                        LetterDefOf.NeutralEvent);
                    Find.LetterStack.ReceiveLetter(letter);
                }
            }
            Map map = Pawn.MapHeld;

            EBSGUtilities.ThingAndSoundMaker(Pawn.Corpse.Position, map, Props.thingSpawnOnFail, Props.thingsToSpawnOnFail, Props.failSound);
            if (Props.deleteOnFailedRevive && Pawn.Corpse != null && !Pawn.Corpse.Destroyed)
                Pawn.Corpse.Destroy();
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref livesLeft, "EBSG_livesLeft", Props.extraLives);
            Scribe_Values.Look(ref progressPercentage, "EBSG_progressPercentage", 0);
            Scribe_Values.Look(ref pawnReviving, "EBSG_pawnReviving", false);
            Scribe_Values.Look(ref revivalProgress, "EBSG_revivalProgress", 0);
            Scribe_Values.Look(ref hoursToRevive, "EBSG_hoursToRevive", 0);
            Scribe_Values.Look(ref deathTile, "EBSG_deathTile", 0);
        }
    }
}
