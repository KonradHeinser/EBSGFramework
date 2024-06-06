using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class GatherOption
    {
        public IntRange amountOnFind;

        public ThingDef thing;

        public ThingDef stuff = null; // Should remain null unless the find is stuffable

        public float weight = 1;

        public float allowanceUsed = 1;

        public List<TerrainDef> viableTerrain; // This is specifically for the terrain it is on

        public int minTemp = -100;

        public int maxTemp = 100;

        public List<TerrainDef> forbiddenTerrain;

        public List<List<TerrainDistance>> nearbyTerrainsNeeded;

        public List<BiomeDef> validBiomes;

        public List<BiomeDef> forbiddenBiomes;
    }
}
