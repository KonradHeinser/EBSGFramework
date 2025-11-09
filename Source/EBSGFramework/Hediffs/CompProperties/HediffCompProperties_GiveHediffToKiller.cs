using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_GiveHediffToKiller : HediffCompProperties
    {
        public float initialSeverity = 1;
        
        public float addedSeverity = 1;

        public HediffDef hediff;
        
        public SuccessChance successChance;
        
        public TargetingParameters targetParams = null;
        
        public List<HediffDef> causes = new List<HediffDef>();
        
        public CheckType causeCheck = CheckType.Required;

        public HediffCompProperties_GiveHediffToKiller()
        {
            compClass = typeof(HediffComp_GiveHediffToKiller);
        }
    }
}