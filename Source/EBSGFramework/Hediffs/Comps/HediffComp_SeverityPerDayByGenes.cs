using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityPerDayByGenes : HediffComp
    {
        private HediffCompProperties_SeverityPerDayByGenes Props => (HediffCompProperties_SeverityPerDayByGenes)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (Props.geneEffects.NullOrEmpty())
                parent.AddedHediffError(parent.pawn);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(200))
                SetSeverity(parent, Pawn, Props.geneEffects, Props.baseSeverity, Props.baseSeverityStatFactor, Props.geneEffectStatFactor, Props.globalStatFactor);
        }

        public static void SetSeverity(Hediff hediff, Pawn pawn, List<GeneEffect> geneEffects, float baseSeverity = 1,
            StatDef baseSeverityStatFactor = null, StatDef geneEffectStatFactor = null, StatDef globalStatFactor = null)
        {
            float newSeverity = baseSeverity * pawn.StatFactorOrOne(baseSeverityStatFactor);

            foreach (GeneEffect geneEffect in geneEffects)
            {
                if (pawn.HasRelatedGene(geneEffect.gene) && pawn.PawnHasAnyOfGenes(out var anyOfGene, geneEffect.anyOfGene) && pawn.PawnHasAllOfGenes(geneEffect.allOfGene))
                {
                    newSeverity += geneEffect.effect * pawn.StatFactorOrOne(geneEffect.statFactor) * pawn.StatFactorOrOne(geneEffectStatFactor);
                }
            }

            hediff.Severity += newSeverity * pawn.StatFactorOrOne(globalStatFactor) * 0.003334f;
        }
    }
}
