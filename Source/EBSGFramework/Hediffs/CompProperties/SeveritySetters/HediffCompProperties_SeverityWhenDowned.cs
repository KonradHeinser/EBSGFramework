using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityWhenDowned : HediffCompProperties
    {
        public float severity = 1f;

        public float? nonDownedSeverity;

        public HediffCompProperties_SeverityWhenDowned()
        {
            compClass = typeof(HediffComp_SeverityWhenDowned);
        }
    }
}