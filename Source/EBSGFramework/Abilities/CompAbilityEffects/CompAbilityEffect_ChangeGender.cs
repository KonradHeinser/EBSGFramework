using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ChangeGender : CompAbilityEffect
    {
        private new CompProperties_AbilityChangeGender Props => (CompProperties_AbilityChangeGender)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (parent.pawn.gender != Gender.None)
                parent.pawn.CheckGender(Props.caster, Props.keepName);
            if (target.Thing is Pawn pawn && pawn.gender != Gender.None)
                pawn.CheckGender(Props.target, Props.keepName);
        }
    }
}