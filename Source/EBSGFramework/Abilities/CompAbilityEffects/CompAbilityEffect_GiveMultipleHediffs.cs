using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveMultipleHediffs : CompAbilityEffect_WithDuration
    {
        public new CompProperties_AbilityGiveMultipleHediffs Props => (CompProperties_AbilityGiveMultipleHediffs)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn t = target.Pawn;
            Props.hediffsToGive?.GiveHediffs(parent.pawn, t, GetDurationSeconds(parent.pawn).SecondsToTicks(),
                t != null ? GetDurationSeconds(target.Pawn).SecondsToTicks() : -1, Props.psychic);
        }
    }
}
