using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityBySeason : HediffCompProperties
    {
        public float defaultSeverity = 0.5f;
        
        public List<SeasonLink> seasonEffects = new List<SeasonLink>();
        
        public HediffCompProperties_SeverityBySeason()
        {
            compClass = typeof(HediffComp_SeverityBySeason);
        }
    }
}