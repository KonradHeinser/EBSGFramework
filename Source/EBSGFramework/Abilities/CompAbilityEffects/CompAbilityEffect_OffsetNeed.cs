﻿using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class CompAbilityEffect_OffsetNeed : CompAbilityEffect
    {
        public new CompProperties_AbilityOffsetNeed Props => (CompProperties_AbilityOffsetNeed)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (target.IsValid && target.HasThing && target.Thing is Pawn pawn && (!Props.psychic || pawn.GetStatValue(StatDefOf.PsychicSensitivity) > 0) && pawn.needs != null)
                EBSGUtilities.HandleNeedOffsets(pawn, Props.needOffsets, Props.preventRepeats);

            if (parent.pawn != null || (Props.psychic && parent.pawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)) return;
            EBSGUtilities.HandleNeedOffsets(parent.pawn, Props.casterNeedOffsets, Props.preventRepeats);
        }
    }
}
