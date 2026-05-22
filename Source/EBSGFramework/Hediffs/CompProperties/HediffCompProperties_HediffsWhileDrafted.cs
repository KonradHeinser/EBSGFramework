using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HediffsWhileDrafted : HediffCompProperties
    {
        public List<HediffToParts> draftedHediffs;
        
        public List<HediffToParts> undraftedHediffs;

        public HediffCompProperties_HediffsWhileDrafted()
        {
            compClass = typeof(HediffComp_HediffsWhileDrafted);
        }
    }
}