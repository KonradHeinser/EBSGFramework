using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_BloodDrain : CompAbilityEffect_BloodfeederBite
    {
        public new CompProperties_AbilityBloodDrain Props => (CompProperties_AbilityBloodDrain)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn pawn = target.Pawn;
            Pawn caster = parent.pawn;

            if (pawn != null)
            {
                if (pawn != caster || Props.damageSelf == true)
                {
                    SanguophageUtility.DoBite(parent.pawn, pawn, Props.hemogenGain, Props.nutritionGain, Props.targetBloodLoss, Props.resistanceGain, Props.bloodFilthToSpawnRange, Props.thoughtDefToGiveTarget, Props.opinionThoughtDefToGiveTarget);
                }
                Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodfeederMark);
                if (firstHediffOfDef != null)
                {
                    pawn.health.RemoveHediff(firstHediffOfDef);
                }
                if (Props.replacementHediff != null)
                {
                    pawn.health.AddHediff(Props.replacementHediff, ExecutionUtility.ExecuteCutPart(pawn));
                }
                if (Props.hediffToSelf != null)
                {
                    caster.health.AddHediff(Props.hediffToSelf);
                }
            }
        }
    }
}
