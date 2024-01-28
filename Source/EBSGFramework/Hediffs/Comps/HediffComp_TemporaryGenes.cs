using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_TemporaryGenes : HediffComp
    {
        public HediffCompProperties_TemporaryGenes Props => (HediffCompProperties_TemporaryGenes)props;

        public List<GeneDef> addedGenes;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (addedGenes == null)
            {
                addedGenes = new List<GeneDef>();
            }

            if (Props.genesAtSeverities.NullOrEmpty())
            {
                Log.Error(Def + " doesn't have any lists to use in HediffCompProperties_TemporaryGenes. Deleting hediff to avoid more errors.");
                parent.pawn.health.RemoveHediff(parent);
                return;
            }

            foreach (GenesAtSeverity geneSet in Props.genesAtSeverities)
            {
                if (parent.Severity > geneSet.minSeverity && parent.Severity < geneSet.maxSeverity)
                {
                    if (EBSGUtilities.EquivalentGeneLists(addedGenes, geneSet.genes)) return;
                    EBSGUtilities.RemoveGenesFromPawn(parent.pawn, addedGenes);
                    addedGenes.Clear();
                    addedGenes = EBSGUtilities.AddGenesToPawn(parent.pawn, geneSet.xenogenes, geneSet.genes);
                    return;
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (!parent.pawn.IsHashIntervalTick(100)) return;

            foreach (GenesAtSeverity geneSet in Props.genesAtSeverities)
            {
                if (parent.Severity > geneSet.minSeverity && parent.Severity < geneSet.maxSeverity)
                {
                    if (EBSGUtilities.EquivalentGeneLists(addedGenes, geneSet.genes)) return;
                    EBSGUtilities.RemoveGenesFromPawn(parent.pawn, addedGenes);
                    addedGenes.Clear();
                    addedGenes = EBSGUtilities.AddGenesToPawn(parent.pawn, geneSet.xenogenes, geneSet.genes);
                    return;
                }
            }
        }


        public override void CompExposeData()
        {
            Scribe_Values.Look(ref addedGenes, "EBSG_AddedGenes", new List<GeneDef>());
        }
    }
}
