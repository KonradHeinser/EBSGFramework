using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_RemoveHediffsOnRemoval : HediffCompProperties
    {
        public List<HediffToParts> hediffs;

        public HediffCompProperties_RemoveHediffsOnRemoval()
        {
            compClass = typeof(HediffComp_RemoveHediffsOnRemoval);
        }
    }
}
