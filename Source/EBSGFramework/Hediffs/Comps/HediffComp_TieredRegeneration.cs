using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TieredRegeneration : HediffComp
    {
        private HediffCompProperties_TieredRegeneration Props => (HediffCompProperties_TieredRegeneration)props;
        private int regrowTicksRemaining;
        private int healTicksRemaining;
        private bool healInProgress = false;
        private bool healWhileRegrowing = false;
        private bool prioritizeHeal = false;

        // Stats from the current set
        private bool regrowthAllowed = false;
        private bool healAllowed = true;
        private float tempMinSeverity;
        private float tempMaxSeverity;
        public int regrowthInterval;
        public int healInterval;
        public int regrowTicksPerTick = 1;
        public int healTicksPerTick = 1;
        private float healAmount;
        private int repeatCount;
        List<Hediff_Injury> wounds;

        // These are to allow the comp to check if it should be continuing to heal/regrow without having to go through the entire list every time


        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            healWhileRegrowing = Props.healWhileRegrowing;
            prioritizeHeal = Props.prioritizeHeal;
            GetSet();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

            if (parent.Severity < tempMinSeverity || parent.Severity > tempMaxSeverity) // This checks if the hediff is in a new set
            {
                GetSet();
            }
            if (healInProgress)
            {
                // Regrowth stuff
                if (regrowTicksRemaining >= 0)
                {
                    if (missingPart == null || !regrowthAllowed) // If regrowth is no longer possible, quit trying to regrow
                    {
                        regrowTicksRemaining = -1;
                    }
                    else
                    {
                        regrowTicksRemaining -= regrowTicksPerTick;
                        if (regrowTicksRemaining <= 0) // Otherwise, if the ticks have hit zero, give the part back
                        {
                            Pawn.health.RemoveHediff(missingPart);
                            regrowTicksRemaining = -1;
                        }
                    }
                }
                // Heal stuff
                if (healTicksRemaining >= 0)
                {
                    GetWounds();

                    if (wounds.Count == 0 || !healAllowed) // If there are no wounds or healing has been disabled, reset heal stuff
                    {
                        healTicksRemaining = -1;
                    }
                    else
                    {
                        healTicksRemaining -= healTicksPerTick;
                        if (healTicksRemaining <= 0) // If done healing, start grabbing random wounds and healing them
                        {
                            for (int i = 0; i > repeatCount; i++)
                            {
                                wounds.RandomElement().Severity -= healAmount;
                            }
                        }
                    }
                }

                if (regrowTicksRemaining < 0 && healTicksRemaining < 0)
                {
                    healInProgress = false;
                }
                else if (healWhileRegrowing) // If both are supposed to be active at the same time, and one of them is still active, try to activate the other
                {
                    missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);
                    GetWounds();

                    if (regrowTicksRemaining < 0 && missingPart != null && regrowthAllowed) // If the inactive one is regrowth and there is a missing part, start the timer
                    {
                        regrowTicksRemaining = regrowthInterval;
                    }
                    else if (healTicksRemaining < 0 && wounds.Count > 0 && healAllowed) // If healing is the inactive one, and there are wounds to heal, start a timer
                    {
                        healTicksRemaining = healInterval;
                    }
                }
            }
            else
            {
                GetWounds();
                if (healWhileRegrowing) // If both are supposed to be active at the same time try to activate both. It checks if they are permitted just to ensure no weird xml inputs are given
                {
                    if (regrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                    {
                        regrowTicksRemaining = regrowthInterval;
                    }
                    if (healAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                    {
                        healTicksRemaining = healInterval;
                    }
                }
                else if (prioritizeHeal) // If healing is prioritized, try to start that first
                {
                    if (healAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                    {
                        healTicksRemaining = healInterval;
                    }
                    else if (regrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                    {
                        regrowTicksRemaining = regrowthInterval;
                    }
                }
                else // Catch tries to regrow first
                {
                    if (regrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                    {
                        regrowTicksRemaining = regrowthInterval;
                    }
                    if (healAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                    {
                        healTicksRemaining = healInterval;
                    }
                }
                healInProgress |= (healTicksRemaining > 0 || regrowTicksRemaining > 0);
            }
        }

        // Get wounds is the method that checks all of a pawn's hediffs for wounds. This is only used when the pawn is about to be checked to minimize performance impact
        private void GetWounds()
        {
            wounds = new List<Hediff_Injury>();
            for (int i = 0; i < Pawn.health.hediffSet.hediffs.Count; i++)
            {
                Hediff_Injury hediff_Injury = Pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
                if (hediff_Injury != null)
                {
                    wounds.Add(hediff_Injury);
                }
            }
        }
        public void GetSet() // This checks to see if the new regen set allows for regrowing parts. If not, it will stop the current regrowth.
        {
            foreach (RegenSet regenSet in Props.regenSets)
            {
                if (parent.Severity >= regenSet.minSeverity && parent.Severity <= regenSet.maxSeverity)
                {
                    if (regenSet.ticksToRegrowPart > 0)
                    { 
                        regrowthAllowed = true;
                        regrowthInterval = regenSet.ticksToRegrowPart;
                    }
                    else regrowthAllowed = false;


                    if (regenSet.ticksToHealInterval > 0)
                    {
                        healInterval = regenSet.ticksToHealInterval;
                        healAllowed = true; 
                    }
                    else healAllowed = false;
                    healAmount = regenSet.healAmount;
                    repeatCount = regenSet.repeatHealCount;

                    tempMinSeverity = regenSet.minSeverity;
                    tempMaxSeverity = regenSet.maxSeverity;

                    healTicksPerTick = regenSet.healTicksPerTick;
                    regrowTicksPerTick = regenSet.regrowTicksPerTick;

                    healInProgress = healAllowed || regrowthAllowed;
                    break;
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref regrowTicksRemaining, "EBSG_regrowTicksRemaining", -1);
            Scribe_Values.Look(ref healTicksRemaining, "EBSG_healTicksRemaining", -1);
        }
    }
}
