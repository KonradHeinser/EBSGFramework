using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityReloadable : CompProperties_AbilityEffect
    {
        public int maxCharges = 1;

        public ThingDef ammoDef;

        public int ammoPerCharge = 1;

        public int reloadDuration = 60;

        public SoundDef reloadSound;

        public bool disableAutoSearch;

        public bool removeOnceEmpty = false;

        public string noChargesRemaining = "AbilityNoCharges";

        public string remainingCharges = "ChargesRemaining";

        public CompProperties_AbilityReloadable()
        {
            compClass = typeof(CompAbilityEffect_Reloadable);
        }
    }
}
