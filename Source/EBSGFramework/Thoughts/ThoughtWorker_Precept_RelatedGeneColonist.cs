using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_RelatedGeneColonist : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.Faction == null)
                return ThoughtState.Inactive;
            
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension != null)
            {
                bool active = false;

                List<Pawn> pawns = p.MapHeld?.mapPawns?.SpawnedPawnsInFaction(p.Faction);

                if (!pawns.NullOrEmpty())
                    foreach (Pawn pawn in pawns)
                    {
                        if (!pawn.IsColonist || pawn.genes == null) continue;

                        bool t = pawn.HasAnyOfRelatedGene(thoughtExtension.relatedGenes) != thoughtExtension.checkNotPresent;

                        if (t)
                        {
                            Precept_Role role = pawn.Ideo?.GetRole(pawn);
                            if (role?.ideo == p.Ideo && role.def == PreceptDefOf.IdeoRole_Leader)
                                return ThoughtState.ActiveAtStage(2);
                        }

                        active |= t;
                    }

                if (active)
                    return ThoughtState.ActiveAtStage(1);
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }

        public static bool HasRelatedGene(Pawn pawn, GeneDef relatedGene)
        {
            return pawn.HasRelatedGene(relatedGene); // Has related gene checks for biotech active and genes existing
        }
    }
}
