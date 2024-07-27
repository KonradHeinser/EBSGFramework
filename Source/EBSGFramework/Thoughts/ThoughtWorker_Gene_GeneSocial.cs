﻿using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_Gene_GeneSocial : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (def.gender != 0 && otherPawn.gender != def.gender || otherPawn.genes == null || !ModsConfig.BiotechActive)
            {
                return ThoughtState.Inactive;
            }

            if (RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                EBSGExtension extension = def.GetModExtension<EBSGExtension>();

                if (!extension.nullifyingGenes.NullOrEmpty() && !extension.opinionOfAllOthers)
                {
                    foreach (Gene gene in p.genes.GenesListForReading)
                    {
                        if (extension.nullifyingGenes.Contains(gene.def)) return ThoughtState.Inactive;
                    }
                }
                if (!extension.requiredGenes.NullOrEmpty())
                {
                    if (!EBSGUtilities.HasAnyOfRelatedGene(p, extension.requiredGenes))
                        return ThoughtState.Inactive;
                }
                if (!extension.compoundingHatred)
                {
                    if (extension.opinionOfAllOthers)
                    {
                        if (EBSGUtilities.PawnHasAnyOfGenes(otherPawn, out var nullifyingGene, extension.nullifyingGenes)) return ThoughtState.Inactive;
                        if (extension.xenophilobic && p.genes.Xenotype == otherPawn.genes.Xenotype) return ThoughtState.Inactive;
                        if (!extension.checkedGenes.NullOrEmpty())
                        {
                            if (EBSGUtilities.PawnHasAnyOfGenes(otherPawn, out var gene, extension.checkedGenes)) return ThoughtState.ActiveAtStage(0);
                            return ThoughtState.Inactive;
                        }
                        return ThoughtState.ActiveAtStage(0);
                    }
                    if (!extension.checkedGenes.NullOrEmpty())
                    {
                        foreach (Gene gene in otherPawn.genes.GenesListForReading)
                            if (extension.checkedGenes.Contains(gene.def)) return ThoughtState.ActiveAtStage(0);
                    }
                    else
                        Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                }
                else
                {
                    int num = 0;
                    if (extension.opinionOfAllOthers)
                    {
                        if (EBSGUtilities.PawnHasAnyOfGenes(otherPawn, out var gene, extension.nullifyingGenes)) return ThoughtState.Inactive;
                        if (extension.xenophilobic && p.genes.Xenotype == otherPawn.genes.Xenotype) return ThoughtState.Inactive;
                    }
                    if (!extension.checkedGenes.NullOrEmpty())
                    {
                        if (extension.opinionOfAllOthers) num++;
                        foreach (Gene gene in otherPawn.genes.GenesListForReading)
                        {
                            if (extension.checkedGenes.Contains(gene.def)) num++;
                            if (num >= def.stages.Count) return ThoughtState.ActiveAtStage(def.stages.Count - 1);
                        }
                        if (num > 0) return ThoughtState.ActiveAtStage(num - 1);
                    }
                    else if (extension.opinionOfAllOthers) return ThoughtState.ActiveAtStage(0);
                    else
                        Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                }
            }

            return ThoughtState.Inactive;
        }
    }
}
