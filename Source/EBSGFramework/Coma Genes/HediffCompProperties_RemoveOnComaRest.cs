using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemoveOnComaRest : HediffCompProperties
    {
        public List<GeneDef> relatedGenes;

        public HediffCompProperties_RemoveOnComaRest()
        {
            compClass = typeof(HediffComp_RemoveOnComaRest);
        }
    }
}
