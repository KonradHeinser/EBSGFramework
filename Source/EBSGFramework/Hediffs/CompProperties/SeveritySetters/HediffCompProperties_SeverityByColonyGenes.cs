using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByColonyGenes : HediffCompProperties
    {
        public GeneDef gene;

        public List<GeneDef> genes;

        public bool mustHaveAllGenes;

        public bool removeWhenNoGenes;

        public HediffCompProperties_SeverityByColonyGenes()
        {
            compClass = typeof(HediffComp_SeverityByColonyGenes);
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
                yield return error;
            if (gene == null && genes.NullOrEmpty())
                yield return "No gene or genes are not set";
        }
    }
}
