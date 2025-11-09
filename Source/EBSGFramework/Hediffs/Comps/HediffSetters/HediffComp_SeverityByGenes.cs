using Verse;

namespace EBSGFramework
{
    public class HediffComp_SeverityByGenes : HediffComp_SetterBase
    {
        private HediffCompProperties_SeverityByGenes Props => (HediffCompProperties_SeverityByGenes)props;

        private GeneDef lastGene;

        private int lastCount;

        protected override bool DoCheck()
        {
            return lastCount != Pawn.genes.GenesListForReading.Count ||
                Pawn.genes.GenesListForReading.GetLast().def != lastGene;
        }

        protected override void SetSeverity()
        {
            float newSeverity = Props.baseSeverity * Pawn.StatOrOne(Props.baseSeverityStatFactor);

            foreach (GeneEffect geneEffect in Props.geneEffects)
                if (Pawn.HasRelatedGene(geneEffect.gene) && Pawn.PawnHasAnyOfGenes(out var anyOfGene, geneEffect.anyOfGene) && Pawn.PawnHasAllOfGenes(geneEffect.allOfGene))
                    newSeverity += geneEffect.effect * Pawn.StatOrOne(geneEffect.statFactor) * Pawn.StatOrOne(Props.geneEffectStatFactor);

            parent.Severity = newSeverity * Pawn.StatOrOne(Props.globalStatFactor);
            lastGene = Pawn.genes.GenesListForReading.GetLast().def;
            lastCount = Pawn.genes.GenesListForReading.Count;
            ticksToNextCheck = 2500;
        }
    }
}
