using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_Gene_GeneSocial : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if ((def.gender != 0 && otherPawn.gender != def.gender) || otherPawn.genes == null || !ModsConfig.BiotechActive)
            {
                Log.Message("Z");
                return ThoughtState.Inactive;
            }

            if (RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                Log.Message($"{p.LabelShort} may have an opinion of {otherPawn.LabelShort}");
                EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
                if (thoughtExtension != null)
                {
                    Log.Message("A");
                    if (!thoughtExtension.nullifyingGenes.NullOrEmpty() && !thoughtExtension.opinionOfAllOthers
                        && p.HasAnyOfRelatedGene(thoughtExtension.nullifyingGenes))
                        return ThoughtState.Inactive;
                    Log.Message("B");
                    if (!thoughtExtension.requiredGenes.NullOrEmpty() && !p.HasAnyOfRelatedGene(thoughtExtension.requiredGenes))
                        return ThoughtState.Inactive;
                    Log.Message("C");
                    if (!thoughtExtension.compoundingHatred)
                    {
                        Log.Message("D");
                        if (thoughtExtension.opinionOfAllOthers)
                        {
                            Log.Message("E");
                            if (otherPawn.HasAnyOfRelatedGene(thoughtExtension.nullifyingGenes))
                                return ThoughtState.Inactive;
                            Log.Message("F");
                            if (thoughtExtension.xenophilobic && p.genes.Xenotype == otherPawn.genes?.Xenotype)
                                return ThoughtState.Inactive;
                            Log.Message("G");
                            if (!thoughtExtension.relatedGenes.NullOrEmpty())
                            {
                                if (otherPawn.HasAnyOfRelatedGene(thoughtExtension.requiredGenes))
                                    return ThoughtState.ActiveAtStage(0);
                                return ThoughtState.Inactive;
                            }
                            Log.Message("H");
                            return ThoughtState.ActiveAtStage(0);
                        }
                        Log.Message("I?");
                        if (!thoughtExtension.relatedGenes.NullOrEmpty())
                        {     
                            if (otherPawn.HasAnyOfRelatedGene(thoughtExtension?.relatedGenes))
                                return ThoughtState.ActiveAtStage(0);
                        }
                        else
                            Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                    }
                    else
                    {
                        Log.Message("J?");
                        int num = -1;
                        if (thoughtExtension.opinionOfAllOthers)
                        {
                            if (otherPawn.HasAnyOfRelatedGene(thoughtExtension.nullifyingGenes)) return ThoughtState.Inactive;
                            if (thoughtExtension.xenophilobic && p.genes.Xenotype == otherPawn.genes.Xenotype) return ThoughtState.Inactive;
                        }
                        if (!thoughtExtension.relatedGenes.NullOrEmpty())
                        {
                            if (thoughtExtension.opinionOfAllOthers) num++;
                            foreach (Gene gene in otherPawn.genes.GenesListForReading)
                            {
                                if (thoughtExtension.relatedGenes.Contains(gene.def)) num++;
                                if (num >= def.stages.Count) return ThoughtState.ActiveAtStage(def.stages.Count - 1);
                            }
                            if (num >= 0) return ThoughtState.ActiveAtStage(num);
                        }
                        else if (thoughtExtension.opinionOfAllOthers) return ThoughtState.ActiveAtStage(0);
                        else
                            Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                    }
                    Log.Message("K");
                    return ThoughtState.Inactive;
                }

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
                    if (!p.HasAnyOfRelatedGene(extension.requiredGenes))
                        return ThoughtState.Inactive;
                }
                if (!extension.compoundingHatred)
                {
                    if (extension.opinionOfAllOthers)
                    {
                        if (otherPawn.PawnHasAnyOfGenes(out var nullifyingGene, extension.nullifyingGenes)) return ThoughtState.Inactive;
                        if (extension.xenophilobic && p.genes.Xenotype == otherPawn.genes.Xenotype) return ThoughtState.Inactive;
                        if (!extension.checkedGenes.NullOrEmpty())
                        {
                            if (otherPawn.PawnHasAnyOfGenes(out var gene, extension.checkedGenes)) return ThoughtState.ActiveAtStage(0);
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
                        if (otherPawn.PawnHasAnyOfGenes(out var gene, extension.nullifyingGenes)) return ThoughtState.Inactive;
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
