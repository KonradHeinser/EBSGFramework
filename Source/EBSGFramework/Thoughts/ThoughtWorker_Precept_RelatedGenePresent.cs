using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_RelatedGenePresent : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.MapHeld == null)
                return ThoughtState.Inactive;

            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension != null)
            {
                List<Pawn> pawns = p.MapHeld?.mapPawns?.SpawnedPawnsInFaction(p.Faction);

                if (!pawns.NullOrEmpty())
                    foreach (Pawn pawn in pawns)
                        if (((pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony || pawn.IsColonist)) &&
                            thoughtExtension.checkNotPresent != pawn.HasAnyOfRelatedGene(thoughtExtension.relatedGenes))
                            return ThoughtState.ActiveDefault;
                
                return ThoughtState.Inactive;
            }

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (!extension.checkNotPresent)
            {
                foreach (Pawn item in p.MapHeld.mapPawns.AllPawnsSpawned)
                {
                    if (item.HasRelatedGene(extension.relatedGene) && (item.IsPrisonerOfColony || item.IsSlaveOfColony || item.IsColonist))
                    {
                        return ThoughtState.ActiveDefault;
                    }
                }
                return ThoughtState.Inactive;
            }
            foreach (Pawn item in p.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (!item.HasRelatedGene(extension.relatedGene) && (item.IsPrisonerOfColony || item.IsSlaveOfColony || item.IsColonist))
                {
                    return ThoughtState.ActiveDefault;
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
