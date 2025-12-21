using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_CreateLinkedHediff : CompAbilityEffect
    {
        public new CompProperties_CreateLinkedHediff Props => (CompProperties_CreateLinkedHediff)props;

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if (Props.successChance?.hideChance == false && target.Thing != null)
                return "EBSG_SuccessChance".Translate(Math.Round(Props.successChance.Chance(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) * 100, 3));

            return null;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn == null || target.Pawn == parent.pawn) return;

            if (Props.successChance?.Success(parent.pawn, target.Thing == parent.pawn ? null : target.Thing) == false)
                return;

            Pawn targetPawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (Props.hediffOnTarget != null)
            {
                Hediff firstHediffOfDef = targetPawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffOnTarget);
                if (firstHediffOfDef != null)
                {
                    targetPawn.health.RemoveHediff(firstHediffOfDef);
                }
                Hediff targetHediff = targetPawn.CreateComplexHediff(Props.hediffOnTarget.initialSeverity, Props.hediffOnTarget, caster, Props.targetHediffOnBrain ? targetPawn.health.hediffSet.GetBrain() : null);
                
                HediffComp_Disappears hediffComp_Disappears = targetHediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    float num = parent.def.EffectDuration(parent.pawn);
                    if (Props.psychic)
                    {
                        num *= targetPawn.StatOrOne(StatDefOf.PsychicSensitivity);
                        num *= caster.StatOrOne(StatDefOf.PsychicSensitivity);
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
                Hediff casterHediff = caster.CreateComplexHediff(Props.hediffOnCaster.initialSeverity, Props.hediffOnCaster, targetPawn, Props.casterHediffOnBrain ? caster.health.hediffSet.GetBrain() : null);
                
                HediffComp_Disappears hediffComp_Disappears = casterHediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    float num = parent.def.EffectDuration(parent.pawn);
                    if (Props.psychic)
                    {
                        num *= caster.StatOrOne(StatDefOf.PsychicSensitivity);
                        num *= targetPawn.StatOrOne(StatDefOf.PsychicSensitivity);
                    }
                    hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                }
                caster.health.AddHediff(casterHediff);
            }
        }
    }
}
