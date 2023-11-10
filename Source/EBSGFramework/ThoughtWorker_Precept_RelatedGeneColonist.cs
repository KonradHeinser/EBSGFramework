using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_RelatedGeneColonist : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive || p.IsBloodfeeder() || p.Faction == null)
            {
                return ThoughtState.Inactive;
            }
            _ = p.Ideo;
            bool flag = false;
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            foreach (Pawn item in p.MapHeld.mapPawns.SpawnedPawnsInFaction(p.Faction))
            {
                if (!extension.checkNotPresent)
                {
                    if (HasRelatedGene(item, extension.relatedGene))
                    {
                        flag = true;
                        Precept_Role precept_Role = item.Ideo?.GetRole(item);
                        if (precept_Role != null && precept_Role.ideo == p.Ideo && precept_Role.def == PreceptDefOf.IdeoRole_Leader)
                        {
                            return ThoughtState.ActiveAtStage(2);
                        }
                    }
                }
                else
                {
                    if (item.IsColonist && (!HasRelatedGene(item, extension.relatedGene)))
                    {
                        flag = true;
                        Precept_Role precept_Role = item.Ideo?.GetRole(item);
                        if (precept_Role != null && precept_Role.ideo == p.Ideo && precept_Role.def == PreceptDefOf.IdeoRole_Leader)
                        {
                            return ThoughtState.ActiveAtStage(2);
                        }
                    }
                }
            }
            if (flag)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            return ThoughtState.ActiveAtStage(0);
        }

        public static bool HasRelatedGene(Pawn pawn, GeneDef relatedGene)
        {
            if (!ModsConfig.BiotechActive || pawn.genes == null)
            {
                return false;
            }
            return pawn.genes.HasGene(relatedGene);
        }
    }
}
