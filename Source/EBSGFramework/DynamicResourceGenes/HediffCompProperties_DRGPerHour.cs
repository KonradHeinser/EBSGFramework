using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_DRGPerHour : HediffCompProperties
    {
        public List<GeneLinker> resourcesPerHour;

        public HediffCompProperties_DRGPerHour()
        {
            compClass = typeof(HediffComp_DRGPerHour);
        }
    }
}
