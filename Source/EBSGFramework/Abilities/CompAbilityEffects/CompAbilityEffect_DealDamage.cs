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
            target.Thing?.TakeDamage(new DamageInfo(Props.def, Props.amount, Props.armorPenetration,
                instigator: parent.pawn, intendedTarget: target.Thing));
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
    }
}
