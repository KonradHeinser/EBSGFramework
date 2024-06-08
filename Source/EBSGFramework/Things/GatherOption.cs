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

        public int nearbyWaterTilesNeeded = 0; // Makes a requirement that some type of water be nearby when above 0. Used for requiring the thing be near a lake, ocean, etc

        public float maxWaterDistance = 1.9f; // Sets the search radius

        public List<List<TerrainDistance>> nearbyTerrainsNeeded;

        public List<BiomeDef> validBiomes; // Not presently in use. Left for potential expansion in the future

        public List<BiomeDef> forbiddenBiomes; // Not presently in use. Left for potential expansion in the future
    }
}
