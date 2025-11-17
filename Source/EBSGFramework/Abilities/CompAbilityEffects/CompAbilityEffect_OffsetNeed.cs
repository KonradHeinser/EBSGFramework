using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_OffsetNeed : CompAbilityEffect
    {
        public new CompProperties_AbilityOffsetNeed Props => (CompProperties_AbilityOffsetNeed)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.IsValid && target.HasThing && target.Thing is Pawn pawn && (!Props.psychic || pawn.GetStatValue(StatDefOf.PsychicSensitivity) > 0) && pawn.needs != null)
                pawn.HandleNeedOffsets(Props.needOffsets, Props.preventRepeats);

            if (parent.pawn?.needs == null || (Props.psychic && parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)) return;
            parent.pawn.HandleNeedOffsets(Props.casterNeedOffsets, Props.preventRepeats);
        }
    }
}
