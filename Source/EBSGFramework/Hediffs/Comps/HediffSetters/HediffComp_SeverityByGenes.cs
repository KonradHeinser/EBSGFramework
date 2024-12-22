using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffComp_SeverityByGenes : HediffComp
    {
        private HediffCompProperties_SeverityByGenes Props => (HediffCompProperties_SeverityByGenes)props;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
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

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (Pawn.IsHashIntervalTick(300))
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
            float newSeverity = baseSeverity * pawn.StatFactorOrOne(baseSeverityStatFactor);

            foreach (GeneEffect geneEffect in geneEffects)
            {
                if (pawn.HasRelatedGene(geneEffect.gene) && pawn.PawnHasAnyOfGenes(out var anyOfGene, geneEffect.anyOfGene) && pawn.PawnHasAllOfGenes(geneEffect.allOfGene))
                {
                    newSeverity += geneEffect.effect * pawn.StatFactorOrOne(geneEffect.statFactor) * pawn.StatFactorOrOne(geneEffectStatFactor);
                }
            }

            hediff.Severity = newSeverity * pawn.StatFactorOrOne(globalStatFactor);
        }
    }
}
