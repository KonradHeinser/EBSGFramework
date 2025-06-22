using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_HemogenPerHour : HediffCompProperties
    {
        public FloatRange validSeverity = FloatRange.Zero;

        public float hemogenPerHour = 0f;

        public FloatRange validHemogen = FloatRange.Zero;

        public bool removeWhenLimitsPassed = false;

        public HediffCompProperties_HemogenPerHour()
        {
            compClass = typeof(HediffComp_HemogenPerHour);
        }
    }
}
