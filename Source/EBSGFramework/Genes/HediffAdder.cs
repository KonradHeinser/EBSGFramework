using Verse;
using System.Collections.Generic;

// I made this because the VFE hediff adder makes the hediffs pop up again if any new gene is added through any other method, like a gene randomizing hediff as a wild example
namespace EBSGFramework
{
    public class HediffAdder : SpawnAgeLimiter
    {
        public EBSGExtension extension;

        public override void PostAdd()
        {
            if (!Active || Overridden) return;
            base.PostAdd();
            HediffAdding(pawn, this);
        }

        public override void PostRemove()
        {
            base.PostRemove();
            HediffRemoving(pawn, this);
        }

        public static void HediffAdding(Pawn pawn, Gene gene)
        {
            EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
            if (extension != null && !extension.hediffsToApply.NullOrEmpty())
            {
                EBSGUtilities.AddHediffToParts(pawn, extension.hediffsToApply);
                if (extension.vanishingGene) pawn.genes.RemoveGene(gene);
            }
        }

        public static void HediffRemoving(Pawn pawn, Gene gene)
        {
            EBSGExtension extension = gene.def.GetModExtension<EBSGExtension>();
            if (extension != null && !extension.vanishingGene && !extension.hediffsToApply.NullOrEmpty())
                EBSGUtilities.RemoveHediffsFromParts(pawn, extension.hediffsToApply);
        }
    }
}
