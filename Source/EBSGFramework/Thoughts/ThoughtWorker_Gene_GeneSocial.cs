using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ThoughtWorker_Gene_GeneSocial : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if (def.gender != 0 && otherPawn.gender != def.gender)
            {
                return ThoughtState.Inactive;
            }

            if (RelationsUtility.PawnsKnowEachOther(p, otherPawn))
            {
                EBSGExtension extension = def.GetModExtension<EBSGExtension>();

                if (extension.nullifyingGenes != null)
                {
                    foreach (Gene gene in p.genes.GenesListForReading)
                    {
                        if (extension.nullifyingGenes.Contains(gene.def)) return ThoughtState.Inactive;
                    }
                }

                if (!extension.compoundingHatred)
                {
                    if (extension.checkedGenes != null)
                    {
                        foreach (Gene gene in otherPawn.genes.GenesListForReading)
                        {
                            if (extension.checkedGenes.Contains(gene.def)) return ThoughtState.ActiveAtStage(0);
                        }
                    }
                    else
                    {
                        Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                    }
                }
                else
                {
                    int num = 0;
                    if (extension.checkedGenes != null)
                    {
                        foreach (Gene gene in otherPawn.genes.GenesListForReading)
                        {
                            if (extension.checkedGenes.Contains(gene.def)) num++;
                            if (num >= extension.maxStages) return ThoughtState.ActiveAtStage(extension.maxStages - 1);
                        }
                        return ThoughtState.ActiveAtStage(num - 1);
                    }
                    else
                    {
                        Log.Error(def + " doesn't have any checked genes, meaning it will always be inactive");
                    }
                }
            }

            return ThoughtState.Inactive;
        }
    }
}
