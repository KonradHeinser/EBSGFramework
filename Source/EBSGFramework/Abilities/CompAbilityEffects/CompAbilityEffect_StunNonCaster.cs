using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_StunNonCaster : CompAbilityEffect_WithDuration
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.HasThing)
            {
                base.Apply(target, dest);
                if (target.Thing is Pawn pawn && pawn != parent.pawn)
                {
                    pawn.stances.stunner.StunFor(GetDurationSeconds(pawn).SecondsToTicks(), parent.pawn, false);
                }
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            // Only castable if it's a ranged ability or a nearby enemy is here (I assume that's what the fireburst check does)
            return parent.def.targetRequired || target.Pawn?.TargetCurrentlyAimingAt == parent.pawn;
        }
    }
}
