using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HemogenPerHour : HediffCompProperties
    {
        public float minSeverity = 0f;

        public float maxSeverity = 99999f;

        public float hemogenPerHour = 0f;

        public float minHemogen = 0f;

        public float maxHemogen = 1f;

        public bool removeWhenLimitsPassed = false;

        public HediffCompProperties_HemogenPerHour()
        {
            compClass = typeof(HediffComp_HemogenPerHour);
        }
    }
}
