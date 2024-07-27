using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class RandomXenoGenes
    {
        public float weightOfGeneSet = 1;
        public List<GeneDef> geneSet;
        public bool reverseInheritence = false; // If the normal hediff comp says to make the genes inheritable (germline), making this true would make this specific set uninheritable(xenogene)
        public bool alwaysRemoveGenes = false; // Overrides removeGenesFromOtherLists for this list specifically
    }
}
