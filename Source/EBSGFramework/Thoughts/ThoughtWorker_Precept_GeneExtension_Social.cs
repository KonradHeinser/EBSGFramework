using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_GeneExtension_Social : ThoughtWorker_Precept_Social
    {
        public static GeneDef relatedGene;
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
            {
                return ThoughtState.Inactive;
            }
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (!extension.checkNotPresent)
            {
                return HasRelatedGene(otherPawn, extension.relatedGene);
            }
            else
            {
                return !HasRelatedGene(otherPawn, extension.relatedGene);
            }
        }

        public static bool HasRelatedGene(Pawn pawn, GeneDef relatedGene)
        {
            return EBSGUtilities.HasRelatedGene(pawn, relatedGene); // Has related gene checks for biotech active and genes existing
        }
    }
}
