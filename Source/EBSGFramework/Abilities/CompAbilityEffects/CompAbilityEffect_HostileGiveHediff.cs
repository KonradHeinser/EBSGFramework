using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_HostileGiveHediff : CompAbilityEffect_GiveHediff
    {
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (target.Pawn?.TargetCurrentlyAimingAt != parent.pawn)
                return false;
            return base.AICanTargetNow(target);
        }
    }
}
