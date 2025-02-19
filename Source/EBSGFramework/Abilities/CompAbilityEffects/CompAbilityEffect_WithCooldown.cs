using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_WithCooldown : CompAbilityEffect
    {
        public new CompProperties_AbilityWithCooldown Props => (CompProperties_AbilityWithCooldown)props;

        public void Finished(LocalTargetInfo target)
        {
            Stance_Cooldown cooldown = new Stance_Cooldown(Props.cooldownTicks, target, null)
            {
                neverAimWeapon = true
            };
            parent.pawn.stances.SetStance(cooldown);
        }
    }
}
