﻿using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class JoyGiver_RelaxInWater : JoyGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.MapHeld == null || !def.HasModExtension<EBSGExtension>()) return null;
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();

            if (!EBSGUtilities.HasAnyOfRelatedGene(pawn, extension.relatedGenes)) return null;
            if (EBSGUtilities.BadWeather(pawn.Map)) return null;

            if (!RCellFinder.TryFindRandomCellNearWith(pawn.Position, (IntVec3 p) => p.GetTerrain(pawn.Map).IsWater, pawn.Map, out IntVec3 result)) return null;
            return JobMaker.MakeJob(def.jobDef, result);
        }
    }
}
