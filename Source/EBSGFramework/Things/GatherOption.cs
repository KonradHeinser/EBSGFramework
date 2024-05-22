using Verse;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class GatherOption
    {
        public IntRange amountOnFind;

        public ThingDef thing;

        public float weight = 1;

        public float allowanceUsed = 1;

        public List<TerrainDef> viableTerrain;

        public List<TerrainDef> forbiddenTerrain;

        public List<BiomeDef> validBiomes;

        public List<BiomeDef> forbiddenBiomes;
    }
}
