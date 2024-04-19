using Verse;
using System.Collections.Generic;

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
    }
}
