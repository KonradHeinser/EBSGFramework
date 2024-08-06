using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_EBSGPlaceworker : CompProperties
    {
        public int nearbyWaterTilesNeeded = 0; // Makes a requirement that some type of water be nearby when above 0. Used for requiring the thing be near a lake, ocean, etc

        public float maxWaterDistance = 1.9f; // Sets the search radius

        public List<List<TerrainDistance>> nearbyTerrainsNeeded;

        public CompProperties_EBSGPlaceworker()
        {
            compClass = typeof(CompEBSGPlaceworker);
        }
    }
}
