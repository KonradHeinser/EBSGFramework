using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffComp_TieredRegeneration : HediffComp
    {
        private RegenSet curSet = null; 
        private HediffCompProperties_TieredRegeneration Props => (HediffCompProperties_TieredRegeneration)props;
        private int regrowTicksRemaining;
        private int healTicksRemaining;
        private bool healInProgress;
        private bool healWhileRegrowing;
        private bool prioritizeHeal;

        private List<Hediff_Injury> wounds;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            healWhileRegrowing = Props.healWhileRegrowing;
            prioritizeHeal = Props.prioritizeHeal; 
            curSet = Props.regenSets.FirstOrDefault(r => r.validSeverity.ValidValue(parent.Severity));
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (curSet?.validSeverity.ValidValue(parent.Severity) != true) // This checks if the hediff is in a new set
                curSet = Props.regenSets.FirstOrDefault(r => r.validSeverity.ValidValue(parent.Severity));

            if (healInProgress)
            {
                // Regrowth stuff
                if (regrowTicksRemaining >= 0)
                {
                    Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

                    if (missingPart == null || !curSet.RegrowthAllowed) // If regrowth is no longer possible, quit trying to regrow
                        regrowTicksRemaining = -1;
                    else
                    {
                        regrowTicksRemaining -= curSet.regrowTicksPerTick * delta;
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
                    if (wounds.Count == 0 || !curSet.HealAllowed) // If there are no wounds or healing has been disabled, reset heal stuff
                        healTicksRemaining = -1;
                    else
                    {
                        healTicksRemaining -= curSet.healTicksPerTick * delta;
                        if (healTicksRemaining <= 0) // If done healing, start grabbing random wounds and healing them
                            for (int i = 0; i < curSet.repeatHealCount; i++)
                            {
                                var wound = wounds.RandomElement();
                                wound.Heal(curSet.healAmount);
                                if (wound.Severity <= 0f)
                                {
                                    wounds.Remove(wound);
                                    if (wounds.NullOrEmpty())
                                        break;
                                }
                            }
                    }
                }

                if (regrowTicksRemaining < 0 && healTicksRemaining < 0)
                    healInProgress = false;
                else if (healWhileRegrowing) // If both are supposed to be active at the same time, and one of them is still active, try to activate the other
                {
                    if (regrowTicksRemaining < 0 && curSet.RegrowthAllowed) // If the inactive one is regrowth and there is a missing part, start the timer
                    {
                        Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);
                        if (missingPart != null)
                            regrowTicksRemaining = curSet.ticksToRegrowPart;
                    }
                    else if (healTicksRemaining < 0 && curSet.HealAllowed) // If healing is the inactive one, and there are wounds to heal, start a timer
                    {
                        GetWounds();
                        if (wounds.Count > 0)
                            healTicksRemaining = curSet.ticksToHealInterval;
                    }
                }
            }
            else
            {
                GetWounds();
                Hediff missingPart = Pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.MissingBodyPart);

                if (healWhileRegrowing) // If both are supposed to be active at the same time try to activate both. It checks if they are permitted just to ensure no weird xml inputs are given
                {
                    if (curSet.RegrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                        regrowTicksRemaining = curSet.ticksToRegrowPart;
                    if (curSet.HealAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                        healTicksRemaining = curSet.ticksToHealInterval;
                }
                else if (prioritizeHeal) // If healing is prioritized, try to start that first
                {
                    if (curSet.HealAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                        healTicksRemaining = curSet.ticksToHealInterval;
                    else if (curSet.RegrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                        regrowTicksRemaining = curSet.ticksToRegrowPart;
                }
                else // Catch tries to regrow first
                {
                    if (curSet.RegrowthAllowed && missingPart != null) // If regrowth is permitted and there is a missing part, start the timer
                        regrowTicksRemaining = curSet.ticksToRegrowPart;
                    if (curSet.HealAllowed && wounds.Count > 0) // If healing is permitted and there are wounds to heal, start a timer
                        healTicksRemaining = curSet.ticksToHealInterval;
                }
                healInProgress |= (healTicksRemaining > 0 || regrowTicksRemaining > 0);
            }
        }
        
        // Get wounds is the method that checks all of a pawn's hediffs for wounds. This is only used when the pawn is about to be checked to minimize performance impact
        private void GetWounds()
        {
            wounds = new List<Hediff_Injury>();
            foreach (var t in Pawn.health.hediffSet.hediffs)
                if (t is Hediff_Injury hediff_Injury) 
                    wounds.Add(hediff_Injury);
        }
        
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref regrowTicksRemaining, "EBSG_regrowTicksRemaining", -1);
            Scribe_Values.Look(ref healTicksRemaining, "EBSG_healTicksRemaining", -1);
        }

        public override string CompDebugString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompDebugString());
            if (!Pawn.Dead)
            {
                if (!healInProgress || curSet == null)
                    stringBuilder.Append("Not currently healing");
                else
                {
                    stringBuilder.AppendLine($"heal : {curSet.healAmount} in {healTicksRemaining}");
                    stringBuilder.AppendLine($"regrowTicksRemaining : {regrowTicksRemaining}");
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
