using System.Collections.Generic;
using RimWorld;
using Verse;

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

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;

            if (geneEffects.NullOrEmpty())
                yield return "geneEffects needs to be set.";
        }
    }
}
