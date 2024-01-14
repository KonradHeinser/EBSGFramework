using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_LoveTheCaster : CompAbilityEffect
    {
        public new CompProperties_LoveTheCaster Props => (CompProperties_LoveTheCaster)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (target.Pawn != null && target.Pawn != parent.pawn && target.Pawn.ageTracker.AgeBiologicalYearsFloat > 16f && parent.pawn.ageTracker.AgeBiologicalYearsFloat > 16f)
            {
                Pawn pawn = target.Pawn;
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffToApply);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
                Hediff_LoveTheCaster hediff_LoveTheCaster = (Hediff_LoveTheCaster)HediffMaker.MakeHediff(Props.hediffToApply, pawn, pawn.health.hediffSet.GetBrain());
                hediff_LoveTheCaster.target = parent.pawn;
                if (Props.psychic)
                {
                    HediffComp_Disappears hediffComp_Disappears = hediff_LoveTheCaster.TryGetComp<HediffComp_Disappears>();
                    if (hediffComp_Disappears != null)
                    {
                        float num = parent.def.EffectDuration(parent.pawn);
                        num *= pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                        hediffComp_Disappears.ticksToDisappear = num.SecondsToTicks();
                    }
                }
                pawn.health.AddHediff(hediff_LoveTheCaster);
            }
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Pawn != null && target.Pawn.ageTracker.AgeBiologicalYearsFloat < 16f)
            {
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityCantApplyTooYoung".Translate(target.Pawn), target.Pawn, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            if (parent.pawn.ageTracker.AgeBiologicalYearsFloat < 16f)
            {
                Messages.Message("CannotUseAbility".Translate(parent.def.label) + ": " + "AbilityCantApplyTooYoung".Translate(parent.pawn), parent.pawn, MessageTypeDefOf.RejectInput, historical: false);
                return false;
            }
            return true;
        }
    }
}
