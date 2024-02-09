using Verse;
using RimWorld;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityPerDayByGenes : HediffCompProperties
    {
        public List<GeneEffect> geneEffects;

        public StatDef baseSeverityStatFactor;

        public StatDef geneEffectStatFactor;

        public StatDef globalStatFactor;

        public float baseSeverity = 1;

        public HediffCompProperties_SeverityPerDayByGenes()
        {
            compClass = typeof(HediffComp_SeverityPerDayByGenes);
        }
    }
}
