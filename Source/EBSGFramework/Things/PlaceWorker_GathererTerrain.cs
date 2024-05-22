using System.Linq;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class PlaceWorker_GathererTerrain : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!checkingDef.blueprintDef.comps.NullOrEmpty() && checkingDef.blueprintDef.HasComp(typeof(CompGathererSpot)))
            {
                CompProperties_GathererSpot gatherComp = checkingDef.blueprintDef.GetCompProperties<CompProperties_GathererSpot>();
                if (gatherComp.nearbyWaterTilesNeeded > 0 && !EBSGUtilities.CheckNearbyWater(loc, map, gatherComp.nearbyWaterTilesNeeded, out int count, gatherComp.maxWaterDistance))
                {
                    return new AcceptanceReport("PlaceWorkerWater".Translate());
                }
            }
            return base.AllowsPlacing(checkingDef, loc, rot, map, thingToIgnore, thing);
        }
    }
}
