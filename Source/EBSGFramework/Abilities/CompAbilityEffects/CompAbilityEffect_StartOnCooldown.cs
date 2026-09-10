using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_StartOnCooldown : CompAbilityEffect
    {
        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            if (parent.UsesCharges)
                parent.RemainingCharges = 0;
            if (parent.HasCooldown)
                parent.StartCooldown(parent.def.cooldownTicksRange.RandomInRange);
        }
    }
}