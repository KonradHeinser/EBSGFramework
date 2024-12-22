using Verse;

namespace EBSGFramework
{
    public class HediffComp_DRGPerHour : HediffComp
    {
        HediffCompProperties_DRGPerHour Props => (HediffCompProperties_DRGPerHour)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(200) && Pawn.genes != null && !Props.resourcesPerHour.NullOrEmpty())
            {
                foreach (GeneLinker linker in Props.resourcesPerHour)
                    if (Pawn.HasRelatedGene(linker.mainResourceGene))
                    {
                        Gene gene = Pawn.genes.GetGene(linker.mainResourceGene);
                        if (gene is ResourceGene resource)
                        {
                            if (resource.Value < linker.minResource || resource.Value > linker.maxResource)
                            {
                                if (linker.removeWhenLimitsPassed)
                                {
                                    Pawn.health.RemoveHediff(parent);
                                    return;
                                }
                                continue;
                            }
                            if (parent.Severity < linker.minSeverity || parent.Severity > linker.maxSeverity)
                                continue;

                            ResourceGene.OffsetResource(Pawn, linker.amount * 0.08f, resource, null, linker.usesGainStat, false, linker.usePassiveStat);
                        }
                    }
            }
        }
    }
}
