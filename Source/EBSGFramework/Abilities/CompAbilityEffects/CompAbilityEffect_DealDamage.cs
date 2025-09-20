using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RimWorld;
using Verse;
using Verse.AI;

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
                if (target.Pawn != null) 
                {
                    if (!Props.bodyParts.NullOrEmpty())
                        part = target.Pawn.GetSemiRandomPartFromList(Props.bodyParts);
                }

                var amount = Props.amount > 0 ? Props.amount : Props.def.defaultDamage;
                var armorPenetration = Props.armorPenetration > 0 ? Props.armorPenetration : Props.def.defaultArmorPenetration;

                target.Thing?.TakeDamage(new DamageInfo(Props.def, amount, armorPenetration,
                    instigator: parent.pawn, hitPart: part, intendedTarget: target.Thing));
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
            if (target.Thing == null)
                return false;
            return base.Valid(target, throwMessages);
        }

        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            if ((Props.chance != 1f || Props.statEffects != null) && target.Thing != null)
                return "EBSG_SuccessChance".Translate(Math.Round(GetChance(target) * 100, 3));
            return null;
        }
    }
}
