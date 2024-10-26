using Verse;
using RimWorld;

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
    }
}
