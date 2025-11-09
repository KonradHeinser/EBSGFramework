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
        private bool healInProgress;
        private bool healWhileRegrowing;
        private bool prioritizeHeal;

        // Stats from the current set
        private bool regrowthAllowed;
        private bool healAllowed = true;
        private FloatRange tempSeverityRange = FloatRange.Zero;
        public int regrowthInterval;
        public int healInterval;
        public int regrowTicksPerTick = 1;
        public int healTicksPerTick = 1;
        private float healAmount;
        private int repeatCount;
        List<Hediff_Injury> wounds;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            healWhileRegrowing = Props.healWhileRegrowing;
            prioritizeHeal = Props.prioritizeHeal;
            GetSet();
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (!tempSeverityRange.Includes(parent.Severity)) // This checks if the hediff is in a new set
                GetSet();

            if (healInProgress)
            {
                // Regrowth stuff
                if (regrowTicksRemaining >= 0)
                {
                    Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

                    if (missingPart == null || !regrowthAllowed) // If regrowth is no longer possible, quit trying to regrow
                    {
                        regrowTicksRemaining = -1;
                    }
                    else
                    {
                        regrowTicksRemaining -= regrowTicksPerTick * delta;
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
                        healTicksRemaining -= healTicksPerTick * delta;
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
                    if (regrowTicksRemaining < 0 && regrowthAllowed) // If the inactive one is regrowth and there is a missing part, start the timer
                    {
                        Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);
                        if (missingPart != null)
                            regrowTicksRemaining = regrowthInterval;
                    }
                    else if (healTicksRemaining < 0 && healAllowed) // If healing is the inactive one, and there are wounds to heal, start a timer
                    {
                        GetWounds();
                        if (wounds.Count > 0)
                            healTicksRemaining = healInterval;
                    }
                }
            }
            else
            {
                GetWounds();
                Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

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

        public override void CompPostTick(ref float severityAdjustment)
        {
            Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

            if (!tempSeverityRange.Includes(parent.Severity)) // This checks if the hediff is in a new set
                GetSet();
            
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
                if (regenSet.validSeverity.ValidValue(parent.Severity))
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

                    tempSeverityRange = regenSet.validSeverity;
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
