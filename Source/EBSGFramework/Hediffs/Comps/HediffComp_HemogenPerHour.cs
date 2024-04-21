using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class HediffComp_HemogenPerHour : HediffComp
    {
        HediffCompProperties_HemogenPerHour Props => (HediffCompProperties_HemogenPerHour)props;

        private Gene_Hemogen cachedGene;

        public Gene_Hemogen Gene
        {
            get
            {
                if (cachedGene == null)
                    cachedGene = Pawn.genes.GetFirstGeneOfType<Gene_Hemogen>();

                return cachedGene;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(200) && Pawn.genes != null && Gene != null)
            {
                try
                {
                    if (Gene.Value < Props.minHemogen || Gene.Value > Props.maxHemogen)
                    {
                        if (Props.removeWhenLimitsPassed)
                            Pawn.health.RemoveHediff(parent);
                        return;
                    }
                }
                catch // On the off chance that cachedGene is holding an invalid gene
                {
                    cachedGene = null;
                    if (Gene == null)
                        return;
                }
                if (parent.Severity < Props.minSeverity || parent.Severity > Props.maxSeverity) return;

                Gene.Value += Props.hemogenPerHour * 0.08f;
            }
        }
    }
}
