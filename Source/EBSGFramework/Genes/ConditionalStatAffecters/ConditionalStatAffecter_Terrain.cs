using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ConditionalStatAffecter_Terrain : ConditionalStatAffecter
    {
        public List<TerrainDef> terrains;

        public bool hateTerrains = false;

        public bool anyWater = false;

        public bool anyNonWater = false;

        public bool defaultActive;

        public string label = null;

        public override string Label => GetLabel();

        private string GetLabel()
        {
            if (label != null) return label.TranslateOrFormat();
            if (anyWater && anyNonWater && !hateTerrains) return "EBSG_Constant".Translate(); // This should never actually be used

            if (terrains.NullOrEmpty())
            {
                if (anyWater) return "EBSG_Water".Translate();
                return "EBSG_NoWater".Translate();
            }
            if (hateTerrains)
            {
                if (anyWater) return "EBSG_WaterHated".Translate();
                if (anyNonWater) return "EBSG_NoWaterHated".Translate();
                return "EBSG_HateTerrain".Translate();
            }
            if (anyWater) return "EBSG_WaterGood".Translate();
            if (anyNonWater) return "EBSG_NoWaterGood".Translate();
            return "EBSG_GoodTerrain".Translate();
        }

        public override bool Applies(StatRequest req)
        {
            if (req.HasThing && req.Thing.Spawned)
            {
                if (!terrains.NullOrEmpty())
                {
                    if (hateTerrains)
                    {
                        // If the terrain list is for disabling terrains, then check water is only ran if the current terrain isn't on the list. 
                        // If anyWater/anyNonWater is being used, then only waters/non-waters need to be included on the terrains list
                        if (!terrains.Contains(req.Thing.Position.GetTerrain(req.Thing.Map))) return CheckWater(req.Thing);
                        return false;
                    }

                    // If not looking for disabling terrains, then any terrain on the list is treated as a success
                    // If anyWater/anyNonWater is being used, then only non-waters/waters need to be included on the terrains list
                    if (terrains.Contains(req.Thing.Position.GetTerrain(req.Thing.Map))) return true;
                    return CheckWater(req.Thing); // If the pawn isn't on a liked terrain, then check for water 
                }
                return CheckWater(req.Thing); // If there isn't a terrains list, then it all depends on if the pawn is or isn't in the water
            }

            return defaultActive;
        }

        private bool CheckWater(Thing thing)
        {
            if (!anyWater && !anyNonWater) return true; // In this situation the terrain list is deciding 
            if (anyWater && anyNonWater) return true; // If it allows all water and non-water, then everything works. This shouldn't happen
            if (anyWater && thing.Position.GetTerrain(thing.Map).IsWater) return true;
            if (anyNonWater && !thing.Position.GetTerrain(thing.Map).IsWater) return true;
            return false;
        }
    }
}
