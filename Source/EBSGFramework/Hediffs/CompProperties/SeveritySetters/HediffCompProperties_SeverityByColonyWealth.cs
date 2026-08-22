using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByColonyWealth : HediffCompProperties
    {
        public SimpleCurve curve;

        public HediffCompProperties_SeverityByColonyWealth()
        {
            compClass = typeof(HediffComp_SeverityByColonyWealth);
        }
    }
}