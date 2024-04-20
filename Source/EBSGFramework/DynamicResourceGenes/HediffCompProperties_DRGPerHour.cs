using Verse;
using System.Collections.Generic;

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
