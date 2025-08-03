using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_SeverityPerIntervalByTerrain : HediffCompProperties
    {
        public int interval = 60;

        public List<TerrainDef> validTerrains;

        public List<string> validTerrainTags;

        public TerrainPollution pollution = TerrainPollution.Ignore;

        public float severityOnValid = 0f;

        public float severityOnInvalid = 0f;

        public HediffCompProperties_SeverityPerIntervalByTerrain()
        {
            compClass = typeof(HediffComp_SeverityPerIntervalByTerrain);
        }
    }
}
