using System.Collections.Generic;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AddItemToInventory : CompProperties_AbilityEffect
    {
        public ThingDef targetThing;
        
        public ThingCategoryDef targetCategory;

        public List<ThingDef> excludeTargetCategory = new List<ThingDef>();

        public int targetCount = 1;

        public ThingDef targetStuffing;

        public ThingDef casterThing;
        
        public ThingCategoryDef casterCategory;
        
        public List<ThingDef> excludeCasterCategory = new List<ThingDef>();

        public int casterCount = 1;

        public ThingDef casterStuffing;

        public bool tryEquip = true;

        public CompProperties_AddItemToInventory()
        {
            compClass = typeof(CompAbilityEffect_AddItemToInventory);
        }
    }
}
