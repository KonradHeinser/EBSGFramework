using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_ToggleHediff : CompAbilityEffect
    {
        public new CompProperties_AbilityToggleHediff Props => (CompProperties_AbilityToggleHediff)props;

        public Pawn Caster => parent.pawn;
        
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn pawn = target.Pawn ?? Caster;
            if (pawn.HasHediff(Props.toggle))
                pawn.RemoveHediffsFromParts(null, Props.toggle);
            else
                pawn.AddHediffToParts(null, Props.toggle);
        }
    }
}