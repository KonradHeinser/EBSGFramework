using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AddItemToInventory : CompProperties_AbilityEffect
    {
        public ThingDef targetThing;

        public int targetCount = 1;

        public ThingDef targetStuffing;

        public ThingDef casterThing;

        public int casterCount = 1;

        public ThingDef casterStuffing;

        public CompProperties_AddItemToInventory()
        {
            compClass = typeof(CompAbilityEffect_AddItemToInventory);
        }
    }
}
