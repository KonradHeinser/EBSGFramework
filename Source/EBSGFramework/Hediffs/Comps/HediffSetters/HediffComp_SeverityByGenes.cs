using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityByGenes : HediffComp
    {
        private HediffCompProperties_SeverityByGenes Props => (HediffCompProperties_SeverityByGenes)props;

        private GeneDef lastGene = null;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            SetSeverity();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.genes.GenesListForReading.GetLast().def != lastGene || Pawn.IsHashIntervalTick(2500))
                SetSeverity();
        }

        public void SetSeverity()
        {
            float newSeverity = Props.baseSeverity * Pawn.StatOrOne(Props.baseSeverityStatFactor);

            foreach (GeneEffect geneEffect in Props.geneEffects)
                if (Pawn.HasRelatedGene(geneEffect.gene) && Pawn.PawnHasAnyOfGenes(out var anyOfGene, geneEffect.anyOfGene) && Pawn.PawnHasAllOfGenes(geneEffect.allOfGene))
                    newSeverity += geneEffect.effect * Pawn.StatOrOne(geneEffect.statFactor) * Pawn.StatOrOne(Props.geneEffectStatFactor);

            parent.Severity = newSeverity * Pawn.StatOrOne(Props.globalStatFactor);
            lastGene = Pawn.genes.GenesListForReading.GetLast().def;
        }
    }
}
