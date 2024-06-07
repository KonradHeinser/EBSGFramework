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

        public int minTemp = -100;

        public int maxTemp = 100;

        public List<List<TerrainDistance>> nearbyTerrainsNeeded;

        public List<BiomeDef> validBiomes; // Not presently in use. Left for potential expansion in the future

        public List<BiomeDef> forbiddenBiomes; // Not presently in use. Left for potential expansion in the future
    }
}
