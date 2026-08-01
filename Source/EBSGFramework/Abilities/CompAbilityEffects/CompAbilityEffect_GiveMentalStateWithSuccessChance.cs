using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveMentalStateWithSuccessChance : CompAbilityEffect_GiveMentalState
    {
        public CompProperties_AbilityGiveMentalStateWithSuccessChance Success => (CompProperties_AbilityGiveMentalStateWithSuccessChance)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo target2)
        {
            if (Success.successChance?.Success(parent.pawn, target.Thing) != false)
                base.Apply(target, target2);
        }
        
        public override string ExtraLabelMouseAttachment(LocalTargetInfo target)
        {
            return target.Pawn?.mindState != null ? Success.successChance?.ExtraLabelMouseAttachment(parent.pawn, target) : null;
        }
    }
}