using System.Collections.Generic;
using Verse;
using System.Linq;

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
                if (!position.IsValid || position != parent.Position)
                {
                    viableOptions = new List<GatherOption>();

                    foreach (GatherOption option in Props.options)
                        if (!option.nearbyTerrainsNeeded.NullOrEmpty())
                        {
                            bool flag = true;
                            foreach (List<TerrainDistance> terrains in option.nearbyTerrainsNeeded)
                                if (!EBSGUtilities.CheckNearbyTerrain(parent, terrains, out TerrainDef missing, out bool negative))
                                {
                                    flag = false;
                                    break;
                                }
                            if (flag) viableOptions.Add(option);
                        }
                        else
                            viableOptions.Add(option);

                    position = parent.Position;
                }
                return viableOptions;
            }
        }
    }
}
