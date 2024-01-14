using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_OffsetNeed : CompAbilityEffect
    {
        public new CompProperties_AbilityOffsetNeed Props => (CompProperties_AbilityOffsetNeed)props;

        public List<Need> alreadyPickedNeeds;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.IsValid && target.HasThing && target.Thing is Pawn pawn && (!Props.psychic || pawn.GetStatValue(StatDefOf.PsychicSensitivity) > 0))
            {
                if (pawn.needs == null) return;
                EBSGUtilities.HandleNeedOffsets(pawn, Props.needOffsets, Props.preventRepeats);
            }
        }
    }
}
