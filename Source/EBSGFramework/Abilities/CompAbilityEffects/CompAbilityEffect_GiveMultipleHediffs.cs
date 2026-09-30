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
            var t = target.Pawn;
            
            if (Props.successChance?.Success(parent.pawn, t) == false)
                return;
            
            Props.hediffsToGive?.GiveHediffs(parent.pawn, t, GetDurationSeconds(parent.pawn).SecondsToTicks(),
                t != null ? GetDurationSeconds(target.Pawn).SecondsToTicks() : -1, Props.psychic, Props.endOn);
        }
    }
}
