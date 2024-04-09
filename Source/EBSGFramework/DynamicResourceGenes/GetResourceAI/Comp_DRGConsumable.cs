using Verse;
using System;
using UnityEngine;

namespace EBSGFramework
{
    public class Comp_DRGConsumable : ThingComp
    {
        public CompProperties_DRGConsumable Props => (CompProperties_DRGConsumable)props;

        public GeneLinker GetRelatedLinker(Pawn pawn)
        {
            if (pawn == null) return null;

            foreach (GeneLinker linker in Props.resourceOffsets)
                if (EBSGUtilities.HasRelatedGene(pawn, linker.mainResourceGene))
                    return linker;

            return null;
        }

        public int NumberToConsume(Pawn pawn)
        {
            if (pawn == null) return 1;
            int num = 1;

            foreach (GeneLinker linker in Props.resourceOffsets)
                if (EBSGUtilities.HasRelatedGene(pawn, linker.mainResourceGene) && pawn.genes.GetGene(linker.mainResourceGene) is ResourceGene resourceGene)
                    if (Mathf.FloorToInt(resourceGene.AmountMissing / linker.amount) > num)
                        num = Mathf.FloorToInt(resourceGene.AmountMissing / linker.amount);

            return num;
        }
    }
}
