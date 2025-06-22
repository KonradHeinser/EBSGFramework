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
                return ThoughtState.Inactive;
            }

            if (RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                EBSGThoughtExtension thoughtExtension = def.GetModExtension<EBSGThoughtExtension>();
                if (thoughtExtension != null)
                {
                    if (!thoughtExtension.nullifyingGenes.NullOrEmpty() && !thoughtExtension.opinionOfAllOthers
                        && p.HasAnyOfRelatedGene(thoughtExtension.nullifyingGenes))
                        return ThoughtState.Inactive;

                    if (!thoughtExtension.requiredGenes.NullOrEmpty() && !p.HasAnyOfRelatedGene(thoughtExtension.requiredGenes))
                        return ThoughtState.Inactive;

                    if (!thoughtExtension.compoundingHatred)
                    {
                        if (thoughtExtension.opinionOfAllOthers)
                        {
                            if (otherPawn.HasAnyOfRelatedGene(thoughtExtension.nullifyingGenes))
                                return ThoughtState.Inactive;

                            if (thoughtExtension.xenophilobic && p.genes.Xenotype == otherPawn.genes?.Xenotype)
                                return ThoughtState.Inactive;

                            if (!thoughtExtension.relatedGenes.NullOrEmpty())
                            {
                                if (otherPawn.HasAnyOfRelatedGene(thoughtExtension.requiredGenes))
                                    return ThoughtState.ActiveAtStage(0);
                                return ThoughtState.Inactive;
                            }

                            return ThoughtState.ActiveAtStage(0);
                        }

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

                    return ThoughtState.Inactive;
                }
            }

            return ThoughtState.Inactive;
        }
    }
}
