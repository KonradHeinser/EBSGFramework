using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityFromResource : HediffCompProperties
    {
        public float severityPerHourEmpty; // Both empty and full expect severity per hour to be positive

        public float severityPerHourFull; // This only functions if severityPerHourEmpty is at 0(default value)

        public float severityPerHourResource; // Applies severity if energy not empty/full, depending on which of the two is used

        public GeneDef mainResourceGene;

        public HediffCompProperties_SeverityFromResource()
        {
            compClass = typeof(HediffComp_SeverityFromResource);
        }
    }
}
