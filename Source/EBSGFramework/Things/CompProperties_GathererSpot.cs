using System.Collections.Generic;
using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_GathererSpot : CompProperties
    {
        public List<GatherOption> options;

        public float maxAllowance = 1f; // The maximum amount of stuff that can be found after one gather session

        public int nearbyWaterTilesNeeded = 0; // Makes a requirement that some type of water be nearby when above 0. Used for requiring the thing be near a lake, ocean, etc

        public float maxWaterDistance = 1.9f; // Sets the search radius

        public bool allowOverForLastItem = false; // Allows the last item to ignore maxAllowance

        public bool justDoOneOfEachOption = false; // Ignores allowance and attempts to spawn each option once

        public List<List<TerrainDistance>> nearbyTerrainsNeeded;

        public float gatherRadius = 10f; // The area pawns can wander to. Usually not worth changing unless the terrain checks have a notably wider radius

        public bool focusWanderInWater = false; // When true, the gathering will tend towards walking through water

        public IntRange ticksNeededToFindSomething = new IntRange(1500, 2500); // Defaults to taking anywhere from just over half an hour to an hour

        public SoundDef gatheringSound; // Sustainer

        public SoundDef gatheringFinishedSound; // One shot

        public CompProperties_GathererSpot()
        {
            compClass = typeof(CompGathererSpot);
        }
    }
}
