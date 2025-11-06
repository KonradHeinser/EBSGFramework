using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_DRGOnKill : HediffCompProperties
    {
        public List<GeneLinker> resourceOffsets;

        public HediffCompProperties_DRGOnKill()
        {
            compClass = typeof(HediffComp_DRGOnKill);
        }
    }
}
