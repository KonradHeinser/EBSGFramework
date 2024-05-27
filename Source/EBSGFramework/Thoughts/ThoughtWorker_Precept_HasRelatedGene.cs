using System;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class ThoughtWorker_Precept_HasRelatedGene : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            if (!ModsConfig.BiotechActive || !ModsConfig.IdeologyActive)
            {
                return ThoughtState.Inactive;
            }
            EBSGExtension extension = def.GetModExtension<EBSGExtension>();
            if (!extension.checkNotPresent)
            {
                return HasRelatedGene(p, extension.relatedGene);
            }
            else
            {
                return !HasRelatedGene(p, extension.relatedGene);
            }
        }

        public static bool HasRelatedGene(Pawn pawn, GeneDef relatedGene)
        {
            return EBSGUtilities.HasRelatedGene(pawn, relatedGene); // Has related gene checks for biotech active and genes existing
        }
    }
}

