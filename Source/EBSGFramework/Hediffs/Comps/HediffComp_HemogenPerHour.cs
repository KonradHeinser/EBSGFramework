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
                    cachedGene = Pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();

                return cachedGene;
            }
        }

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (Gene != null)
            {
                try
                {
                    if ((!Props.validHemogen.ValidValue(Gene.Value)))
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
                if (!Props.validSeverity.ValidValue(parent.Severity)) return;

                GeneUtility.OffsetHemogen(Pawn, Props.hemogenPerHour / 2500f * (float)delta);
            }
        }
    }
}
