using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class HediffCompProperties_TerrainCostOverride : HediffCompProperties
    {
        public int universalCostOverride = -1;

        public List<TerrainLinker> terrains;

        public HediffCompProperties_TerrainCostOverride()
        {
            compClass = typeof(HediffComp_TerrainCostOverride);
        }
    }
}
