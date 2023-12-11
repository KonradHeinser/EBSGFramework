using System;
using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveMultipleHediffs : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveMultipleHediffs Props => (CompProperties_AbilityGiveMultipleHediffs)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            foreach (HediffToGive hediffToGive in Props.hediffsToGive)
            {
                if (!hediffToGive.onlyApplyToSelf && hediffToGive.applyToTarget)
                {
                    ApplyInner(target.Pawn, parent.pawn, hediffToGive);
                }
                if (hediffToGive.applyToSelf || hediffToGive.onlyApplyToSelf)
                {
                    ApplyInner(parent.pawn, target.Pawn, hediffToGive);
                }
            }
        }

        protected void ApplyInner(Pawn target, Pawn other, HediffToGive hediffToGive)
        {
            if (target == null)
            {
                return;
            }
            Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(hediffToGive.hediffDef);
            if (hediffToGive.replaceExisting)
            {
                if (firstHediffOfDef != null)
                {
                    target.health.RemoveHediff(firstHediffOfDef);
                }
            }
            else
            {
                if (firstHediffOfDef != null)
                {
                    firstHediffOfDef.Severity += hediffToGive.severity;
                    return;
                }
            }
            Hediff hediff = HediffMaker.MakeHediff(hediffToGive.hediffDef, target, hediffToGive.onlyBrain ? target.health.hediffSet.GetBrain() : null);
            HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
            if (hediffComp_Disappears != null)
            {
                hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(target).SecondsToTicks();
            }
            if (hediffToGive.severity >= 0f)
            {
                hediff.Severity = hediffToGive.severity;
            }
            HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
            if (hediffComp_Link != null)
            {
                hediffComp_Link.other = other;
                hediffComp_Link.drawConnection = target == parent.pawn;
            }
            target.health.AddHediff(hediff);
        }
    }
}
