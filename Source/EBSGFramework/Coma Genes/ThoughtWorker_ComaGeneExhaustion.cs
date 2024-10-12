using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_ComaGeneExhaustion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!(p.genes?.GetGene(def.GetModExtension<ComaExtension>()?.relatedGene) is Gene_Coma comaGene))
                return ThoughtState.Inactive;
            return comaGene.ComaNeed.CurLevel == 0f;
        }
    }
}
