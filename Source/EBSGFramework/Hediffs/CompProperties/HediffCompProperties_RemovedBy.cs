using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemovedBy: HediffCompProperties
    {
        public List<GeneDef> genes;

        public HediffCompProperties_RemovedBy()
        {
            compClass = typeof(HediffComp_RemovedBy);
        }
    }
}