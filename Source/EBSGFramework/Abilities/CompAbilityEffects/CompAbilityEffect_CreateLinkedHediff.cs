using RimWorld;
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
                HediffWithTarget targetHediff = (HediffWithTarget)HediffMaker.MakeHediff(Props.hediffOnTarget, targetPawn, Props.targetHediffOnBrain ? targetPawn.health.hediffSet.GetBrain() : null);
                targetHediff.target = caster;
                if (Props.psychic)
                {
                    HediffComp_Disappears hediffComp_Disappears = targetHediff.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears != null)
                    {
                        float num = parent.def.EffectDuration(parent.pawn);
                        num *= targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
                        num *= caster.GetStatValue(StatDefOf.PsychicSensitivity);
                        hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                    }
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
                HediffWithTarget casterHediff = (HediffWithTarget)HediffMaker.MakeHediff(Props.hediffOnCaster, caster, Props.casterHediffOnBrain ? caster.health.hediffSet.GetBrain() : null);
                casterHediff.target = targetPawn;
                if (Props.psychic)
                {
                    HediffComp_Disappears hediffComp_Disappears = casterHediff.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears != null)
                    {
                        float num = parent.def.EffectDuration(parent.pawn);
                        num *= caster.GetStatValue(StatDefOf.PsychicSensitivity);
                        num *= targetPawn.GetStatValue(StatDefOf.PsychicSensitivity);
                        hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                    }
                }
                caster.health.AddHediff(casterHediff);
            }
        }
    }
}
