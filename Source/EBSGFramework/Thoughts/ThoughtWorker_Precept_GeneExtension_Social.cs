using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_GeneExtension_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
                return ThoughtState.Inactive;

            EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
            if (!thoughtExtension?.relatedGenes.NullOrEmpty() == false)
                return thoughtExtension.checkNotPresent != otherPawn.HasAnyOfRelatedGene(thoughtExtension.relatedGenes);

            return ThoughtState.Inactive;
        }
    }
}
