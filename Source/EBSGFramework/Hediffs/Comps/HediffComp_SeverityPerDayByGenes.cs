using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityPerDayByGenes : HediffComp
    {
        private HediffCompProperties_SeverityPerDayByGenes Props => (HediffCompProperties_SeverityPerDayByGenes)props;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(200))
            {
                if (Props.geneEffects.NullOrEmpty())
                {
                    Log.Error(Def + " doesn't have any gene effects specified, so severity cannot be assigned.");
                    parent.Severity = 0;
                }
                else
                {
                    SetSeverity(parent, Pawn, Props.geneEffects, Props.baseSeverity, Props.baseSeverityStatFactor, Props.geneEffectStatFactor, Props.globalStatFactor);
                }
            }
        }

        public static void SetSeverity(Hediff hediff, Pawn pawn, List<GeneEffect> geneEffects, float baseSeverity = 1,
            StatDef baseSeverityStatFactor = null, StatDef geneEffectStatFactor = null, StatDef globalStatFactor = null)
        {
            float newSeverity = baseSeverity * EBSGUtilities.StatFactorOrOne(pawn, baseSeverityStatFactor);

            foreach (GeneEffect geneEffect in geneEffects)
            {
                if (EBSGUtilities.HasRelatedGene(pawn, geneEffect.gene) && EBSGUtilities.PawnHasAnyOfGenes(pawn, geneEffect.anyOfGene) && EBSGUtilities.PawnHasAllOfGenes(pawn, geneEffect.allOfGene))
                {
                    newSeverity += geneEffect.effect * EBSGUtilities.StatFactorOrOne(pawn, geneEffect.statFactor) * EBSGUtilities.StatFactorOrOne(pawn, geneEffectStatFactor);
                }
            }

            hediff.Severity += newSeverity * EBSGUtilities.StatFactorOrOne(pawn, globalStatFactor) * 0.003334f;
        }
    }
}
