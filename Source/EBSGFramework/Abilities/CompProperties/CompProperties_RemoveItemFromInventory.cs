using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_RemoveItemFromInventory : CompProperties_AbilityEffect
    {
        public ThingDef targetThing;

        public int targetCount = 1;

        public bool targetBestEffort = false; // Allows for partial success

        public ThingDef casterThing;

        public int casterCount = 1;

        public bool casterBestEffort = false; // Allows for partial success

        public bool disableGizmoIfCasterInvalid = true;

        public CompProperties_RemoveItemFromInventory()
        {
            compClass = typeof(CompAbilityEffect_RemoveItemFromInventory);
        }
    }
}
