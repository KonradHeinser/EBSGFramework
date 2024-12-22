using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class PlaceWorker_GathererTerrain : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            try
            {
                ThingDef thingy = checkingDef as ThingDef;
                if (!thingy.comps.NullOrEmpty() && thingy.HasComp(typeof(CompGathererSpot)))
                {
                    CompProperties_GathererSpot gatherComp = thingy.GetCompProperties<CompProperties_GathererSpot>();
                    if (gatherComp.nearbyWaterTilesNeeded > 0 && !loc.CheckNearbyWater(map, gatherComp.nearbyWaterTilesNeeded, out int count, gatherComp.maxWaterDistance))
                        return new AcceptanceReport("PlaceWorkerMoreWater".Translate());

                    if (gatherComp.nearbyWaterTilesNeeded < 0 && loc.CheckNearbyWater(map, 1, out int countB, gatherComp.maxWaterDistance))
                        return new AcceptanceReport("PlaceWorkerLessWater".Translate());

                    foreach (List<TerrainDistance> terrains in gatherComp.nearbyTerrainsNeeded)
                        if (!loc.CheckNearbyTerrain(map, terrains, out TerrainDef missingTerrain, out bool negativeTerrain))
                        {
                            if (negativeTerrain)
                                return new AcceptanceReport("PlaceWorkerAvoidTerrain".Translate(missingTerrain.label));
                            if (missingTerrain != null)
                                return new AcceptanceReport("PlaceWorkerMoreTerrain".Translate(missingTerrain.label));
                            return new AcceptanceReport("PlaceWorkerTerrain".Translate());
                        }
                }
            }
            catch
            {
                Log.Error("The EBSG Framework tried to turn " + checkingDef.defName + " into a ThingDef for the GathererTerrain place worker, but instead ended up in the fetal position. Please let the EBSG developer know.");
            }

            return base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        }
    }
}
