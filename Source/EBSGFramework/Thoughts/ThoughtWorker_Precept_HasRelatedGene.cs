using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_HasRelatedGene : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
                return ThoughtState.Inactive;
            
            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (thoughtExtension?.relatedGenes.NullOrEmpty() == false)
                return thoughtExtension.checkNotPresent != p.HasAnyOfRelatedGene(thoughtExtension.relatedGenes);

            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (extension?.relatedGene != null)
                if (!extension.checkNotPresent)
                    return p.HasRelatedGene(extension.relatedGene);
                else
                    return !p.HasRelatedGene(extension.relatedGene);

            return ThoughtState.Inactive;
        }
    }
}

