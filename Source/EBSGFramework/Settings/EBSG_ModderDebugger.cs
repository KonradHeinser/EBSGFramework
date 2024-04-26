using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace EBSGFramework
{
    public static class EBSG_ModderDebugger
    {
        public static void AllPlayerThingsOnColonyMap()
        {
            List<Thing> tmpThings = new List<Thing>();

            Map map = null;

            if (!Find.Maps.NullOrEmpty())
                map = Find.Maps.Where((Map m) => m.IsPlayerHome).ToList()[0];

            ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), tmpThings, false, WealthWatcher.WealthItemsFilter);

            if (!tmpThings.NullOrEmpty())
            {
                Dictionary<ThingDef, int> singleStackResource = new Dictionary<ThingDef, int>();

                tmpThings.SortBy((Thing t) => t.MarketValue * t.stackCount);
                Log.Message("defName : Label : Market Value : Stack Count : x, y, z");
                foreach (Thing thing in tmpThings)
                    if (thing.def.stackLimit <= 1 && !thing.def.IsApparel && !thing.def.IsWeapon)
                        if (singleStackResource.ContainsKey(thing.def)) singleStackResource[thing.def]++;
                        else singleStackResource.Add(thing.def, 1);
                    else
                    {
                        if (thing.Position.x == -1000) continue;
                        Log.Message(thing.def.defName + " : " + thing.Label + " : " + thing.MarketValue + " : " + thing.stackCount + " : " + thing.Position.x + ", " + thing.Position.y + ", " + thing.Position.z);
                    }

                Log.Message("defName : Label : Market Value : Stack Count : x, y, z");
                Log.Message(" ");
                Log.Message("defName : Label : Base Market Value : Number Found");
                foreach (ThingDef thing in singleStackResource.Keys)
                    Log.Message(thing.defName + " : " + thing.label + " : " + thing.BaseMarketValue + " : " + singleStackResource[thing]);
            }
        }
    }
}
