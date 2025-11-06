using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_DRGConsumable : CompProperties
    {
        public List<GeneLinker> resourceOffsets;

        public CompProperties_DRGConsumable()
        {
            compClass = typeof(Comp_DRGConsumable);
        }
    }
}
