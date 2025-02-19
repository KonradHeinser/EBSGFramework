using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_WithCooldown : CompAbilityEffect
    {
        public new CompProperties_AbilityWithCooldown Props => (CompProperties_AbilityWithCooldown)props;

        public override void PostApplied(List<LocalTargetInfo> targets, Map map)
        {
            base.PostApplied(targets, map);
            Stance_Cooldown cooldown = new Stance_Cooldown(Props.cooldownTicks, targets.First(), null)
            {
                neverAimWeapon = true
            };
            parent.pawn.stances.SetStance(cooldown);
        }
    }
}
