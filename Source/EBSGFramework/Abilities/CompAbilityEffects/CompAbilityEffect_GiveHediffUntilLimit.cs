using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_GiveHediffUntilLimit : CompAbilityEffect
    {
        public new CompProperties_AbilityGiveHediffUntilLimit Props => props as CompProperties_AbilityGiveHediffUntilLimit;

        public Pawn Caster => parent.pawn;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            foreach (var h in Props.hediffs)
                h.AffectPawns(Caster, target.Pawn);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return Valid(target, true) && base.CanApplyOn(target, dest);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            return Props.hediffs.Any(h => (h.affectTarget && h.CanAffect(target.Pawn)) || (h.affectSelf && h.CanAffect(Caster))) &&
                   base.Valid(target, throwMessages);
        }
    }
}
