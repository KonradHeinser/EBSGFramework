using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class GeneticMultiplier
    {
        // Not intended for nullifying genes, and if two genes shouldn't stack for this you should probably add exclusion tags to avoid them clashing
        public GeneDef gene;
        public float multiplier;
        public List<GeneDef> nullifyingGenes; // nullifies the effects of this multiplier
    }
}
