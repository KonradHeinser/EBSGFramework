using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityByMap : HediffCompProperties
    {
        public float inPlayerHome;
        
        public float inPocketMap;

        public float inTempIncidentMap;

        public float inMap = 0.5f;

        public float notInMap = 1;
        
        public HediffCompProperties_SeverityByMap()
        {
            compClass = typeof(HediffComp_SeverityByMap);
        }
    }
}