using RimWorld;
using Verse;

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

            return ThoughtState.Inactive;
        }
    }
}

