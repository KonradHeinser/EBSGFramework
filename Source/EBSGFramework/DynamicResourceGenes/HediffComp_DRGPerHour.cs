using Verse;

namespace EBSGFramework
{
    public class HediffComp_DRGPerHour : HediffComp
    {
        HediffCompProperties_DRGPerHour Props => (HediffCompProperties_DRGPerHour)props;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            foreach (GeneLinker linker in Props.resourcesPerHour)
            {
                if (!linker.validSeverity.ValidValue(parent.Severity))
                    continue;
                if (Pawn.HasRelatedGene(linker.mainResourceGene))
                {
                    Gene gene = Pawn.genes.GetGene(linker.mainResourceGene);
                    if (gene is ResourceGene resource)
                    {
                        if (!linker.validResourceLevels.ValidValue(resource.Value))
                        {
                            if (linker.removeWhenLimitsPassed)
                            {
                                Pawn.health.RemoveHediff(parent);
                                return;
                            }
                            continue;
                        }

                        ResourceGene.OffsetResource(Pawn, linker.amount / 2500f * delta, resource, null, linker.usesGainStat, false, linker.usePassiveStat);
                    }
                }
            }
        }
    }
}
