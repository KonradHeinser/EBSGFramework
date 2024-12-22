using System.Collections.Generic;
using Verse;

namespace EBSGFramework
{
    public class CompGathererSpot : ThingComp
    {
        public CompProperties_GathererSpot Props => (CompProperties_GathererSpot)props;

        private IntVec3 position = IntVec3.Invalid;

        private List<GatherOption> viableOptions;

        public List<GatherOption> ViableOptions
        {
            get
            {
                // The hash is for creating a chance for a random "audit", where it redoes everything to check that no weird terrain stuff has been happening. While it can happen up to once per in-game hour, it usually does not
                if (!position.IsValid || position != parent.Position || parent.IsHashIntervalTick(2500))
                {
                    viableOptions = new List<GatherOption>();

                    foreach (GatherOption option in Props.options)
                    {
                        // Checks for situations where nearby water is needed
                        if (option.nearbyWaterTilesNeeded > 0 && !parent.Position.CheckNearbyWater(parent.Map, option.nearbyWaterTilesNeeded, out int countA, option.maxWaterDistance))
                            continue;

                        // Checks for situations where a lack of nearby water is needed
                        if (option.nearbyWaterTilesNeeded < 0 && parent.Position.CheckNearbyWater(parent.Map, 1, out int countB, option.maxWaterDistance))
                            continue;

                        if (!option.nearbyTerrainsNeeded.NullOrEmpty())
                        {
                            bool flag = true;
                            foreach (List<TerrainDistance> terrains in option.nearbyTerrainsNeeded)
                                if (!parent.CheckNearbyTerrain(terrains, out TerrainDef missing, out bool negative))
                                {
                                    flag = false;
                                    break;
                                }
                            if (flag) viableOptions.Add(option);
                        }
                        else
                            viableOptions.Add(option);
                    }

                    position = parent.Position;
                }
                return viableOptions;
            }
        }
    }
}
