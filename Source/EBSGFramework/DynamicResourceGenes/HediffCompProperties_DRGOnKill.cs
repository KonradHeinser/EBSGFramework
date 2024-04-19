using Verse;
using System.Collections.Generic;

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
