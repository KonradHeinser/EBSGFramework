using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityGiveHediffUntilLimit : CompProperties_AbilityEffect
    {
        public HediffDef hediff;

        public FloatRange severity = FloatRange.One;

        public StatDef factorStat;

        public StatDef divisorStat;
        
        public float maxSeverity = 0;
        
        public BodyPartDef part;

        public bool affectTarget = true;
        
        public bool affectSelf = false;
        
        public CompProperties_AbilityGiveHediffUntilLimit()
        {
            compClass = typeof(CompAbilityEffect_GiveHediffUntilLimit);
        }
    }
}
