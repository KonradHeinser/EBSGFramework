using System.Collections.Generic;
using RimWorld;
using Verse;

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

            return ThoughtState.Inactive;
        }
    }
}
