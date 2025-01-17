﻿using RimWorld;
using Verse;
using System;

namespace EBSGFramework
{
    public class CompAbilityEffect_CreateLinkedHediff : CompAbilityEffect
    {
        public new CompProperties_CreateLinkedHediff Props => (CompProperties_CreateLinkedHediff)props;

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (target.Pawn != null && (Props.baseSuccessChance != 1 || Props.casterStatChance != null || Props.targetStatChance != null))
            {
                float finalChance = EBSGUtilities.AbilityCompSuccessChance(Props.baseSuccessChance, parent.pawn, Props.casterStatChance, Props.casterStatDivides, target.Pawn, Props.targetStatChance, Props.targetStatMultiplies);
                return "EBSG_SuccessChance".Translate(Math.Round(finalChance * 100, 3));
            }

            return null;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn == null || target.Pawn == parent.pawn) return;
            if (EBSGUtilities.AbilityCompSucceeds(Props.baseSuccessChance, parent.pawn, Props.casterStatChance, Props.casterStatDivides, target.Pawn, Props.targetStatChance, Props.targetStatMultiplies))
            {
                if (Props.successMessage != null)
                    EBSGUtilities.GiveSimplePlayerMessage(Props.successMessage.TranslateOrLiteral(), parent.pawn, MessageTypeDefOf.SilentInput);
            }
            else
            {
                if (Props.failureMessage != null)
                    EBSGUtilities.GiveSimplePlayerMessage(Props.failureMessage.TranslateOrLiteral(), parent.pawn, MessageTypeDefOf.SilentInput);
                return;
            }

            Pawn targetPawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (Props.hediffOnTarget != null)
            {
                Hediff firstHediffOfDef = targetPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnTarget);
                if (firstHediffOfDef != null)
                {
                    targetPawn.health.RemoveHediff(firstHediffOfDef);
                }
                Hediff targetHediff = EBSGUtilities.CreateComplexHediff(targetPawn, Props.hediffOnTarget.initialSeverity, Props.hediffOnTarget, caster, Props.targetHediffOnBrain ? targetPawn.health.hediffSet.GetBrain() : null);
                
                HediffComp_Disappears hediffComp_Disappears = targetHediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    float num = parent.def.EffectDuration(parent.pawn);
                    if (Props.psychic)
                    {
                        num *= targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
                        num *= caster.GetStatValue(StatDefOf.PsychicSensitivity);
                    }
                    hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                }
                targetPawn.health.AddHediff(targetHediff);
            }

            if (Props.hediffOnCaster != null)
            {

                Hediff firstHediffOfDef = caster.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnCaster);
                if (firstHediffOfDef != null)
                {
                    caster.health.RemoveHediff(firstHediffOfDef);
                }
                Hediff casterHediff = EBSGUtilities.CreateComplexHediff(caster, Props.hediffOnCaster.initialSeverity, Props.hediffOnCaster, targetPawn, Props.casterHediffOnBrain ? caster.health.hediffSet.GetBrain() : null);
                
                HediffComp_Disappears hediffComp_Disappears = casterHediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    float num = parent.def.EffectDuration(parent.pawn);
                    if (Props.psychic)
                    {
                        num *= caster.GetStatValue(StatDefOf.PsychicSensitivity);
                        num *= targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
                    }
                    hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                }
                caster.health.AddHediff(casterHediff);
            }
        }
    }
}
