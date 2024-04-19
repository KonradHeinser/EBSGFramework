using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByAge : HediffCompProperties
    {
        public bool divideByLifespanFactor = true; // Divide by lifespan factor, so the severity of a pawn with a factor of 3 is the same at age 30 as it would be if the pawn was 90 and had a factor of 1

        public bool accountForLifeExpectancyDifference = true; // Makes the severity get multiplied by 80 / race life expectency to make the hediff severity change increase faster for shorter lived beings. Usually only matters if the hediff will be applied to non-humans

        public bool includeMechanoidLifeExpectancy = true;

        public float additionalFactor = 0.1f; // Factor applied after all others so people aren't stuck with exceptionally high severities

        public HediffCompProperties_SeverityByAge()
        {
            compClass = typeof(HediffComp_SeverityByAge);
        }
    }
}
