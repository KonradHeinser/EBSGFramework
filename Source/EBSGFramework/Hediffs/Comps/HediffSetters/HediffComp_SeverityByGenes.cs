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

            foreach (var geneEffect in Props.geneEffects)
                if ((geneEffect.gene == null || Pawn.HasRelatedGene(geneEffect.gene)) &&
                    (geneEffect.anyOfGene.NullOrEmpty() || Pawn.PawnHasAnyOfGenes(out _, geneEffect.anyOfGene)) && Pawn.PawnHasAllOfGenes(geneEffect.allOfGene))
                    newSeverity += geneEffect.effect * Pawn.StatOrOne(geneEffect.statFactor) * Pawn.StatOrOne(Props.geneEffectStatFactor);

            parent.Severity = newSeverity * Pawn.StatOrOne(Props.globalStatFactor);
            lastGene = Pawn.genes.GenesListForReading.GetLast().def;
            lastCount = Pawn.genes.GenesListForReading.Count;
            ticksToNextCheck = 2500;
        }
    }
}
