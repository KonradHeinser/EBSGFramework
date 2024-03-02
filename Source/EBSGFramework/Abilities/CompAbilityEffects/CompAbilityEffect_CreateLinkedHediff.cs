using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_CreateLinkedHediff : CompAbilityEffect
    {
        public new CompProperties_CreateLinkedHediff Props => (CompProperties_CreateLinkedHediff)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.Pawn == null || target.Pawn == parent.pawn) return;
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
