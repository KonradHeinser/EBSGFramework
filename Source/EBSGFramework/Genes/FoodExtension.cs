using RimWorld;
using Verse;
using System.Collections.Generic;

namespace EBSGFramework
{
    public class FoodExtension : DefModExtension
    {
        public bool noStandardFood;

        public List<ThingDef> forbiddenFoods;

        public List<ThingDef> allowedFoods;

        public List<ThingLink> nonIngestibleFoods;

        public FoodTypeFlags foodTypeOverride = FoodTypeFlags.None;
    }
}
