using System;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_DealDamage : CompAbilityEffect
    {
        public new CompProperties_AbilityDealDamage Props => props as CompProperties_AbilityDealDamage;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            if (Rand.Chance(GetChance(target)))
            {
                BodyPartRecord part = null;
                if (target.Pawn != null && !Props.bodyParts.NullOrEmpty()) 
                    part = target.Pawn.GetSemiRandomPartFromList(Props.bodyParts);
                var amount = Props.amount > 0 ? Props.amount : Props.def.defaultDamage;
                var armorPenetration = Props.armorPenetration > 0 ? Props.armorPenetration : Props.def.defaultArmorPenetration;

                var dinfo = new DamageInfo(Props.def, amount, armorPenetration, instigator: parent.pawn, hitPart: part, intendedTarget: target.Thing);
                if (part == null && Props.height.HasValue)
                    dinfo.SetBodyRegion(Props.height.Value, Props.depth);
                
                target.Thing?.TakeDamage(dinfo);
            }
        }

        private float GetChance(LocalTargetInfo target)
        {
            return Math.Min(1, Props.chance * Props.statEffects?.FinalFactor(parent.pawn, target.Thing) ?? 1f);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return base.CanApplyOn(target, dest) && Valid(target, true);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return target.Thing != null && base.Valid(target, throwMessages);
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if ((Props.chance != 1f || Props.statEffects != null) && target.Thing != null)
                return "EBSG_SuccessChance".Translate(Math.Round(GetChance(target) * 100, 3));
            return null;
        }
    }
}
