using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_RelatedGenePresent : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.MapHeld == null)
            {
                return ThoughtState.Inactive;
            }
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (!extension.checkNotPresent)
            {
                foreach (Pawn item in p.MapHeld.mapPawns.AllPawnsSpawned)
                {
                    if (EBSGUtilities.HasRelatedGene(item, extension.relatedGene) && (item.IsPrisonerOfColony || item.IsSlaveOfColony || item.IsColonist))
                    {
                        return ThoughtState.ActiveDefault;
                    }
                }
                return ThoughtState.Inactive;
            }
            foreach (Pawn item in p.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (!EBSGUtilities.HasRelatedGene(item, extension.relatedGene) && (item.IsPrisonerOfColony || item.IsSlaveOfColony || item.IsColonist))
                {
                    return ThoughtState.ActiveDefault;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
