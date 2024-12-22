using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityReloadable : CompProperties_AbilityEffect
    {
        // Maximum amount of charges stored
        public int maxCharges = 1;
        // Amount of ammo spent per charge
        public int ammoPerCharge = 1;
        // Whenever the ability is removed upon all charges being spent
        public bool removeOnceEmpty = false;
        // ThingDef of an item that refills this ability
        public ThingDef ammoDef;
        // How long it takes to reload
        public int reloadDuration = 60;
        // Sound that's played upon reloading
        public SoundDef reloadSound;
        // String that's displayed when no charges are remaining
        public string noChargesRemaining = "AbilityNoCharges";
        // String used to display how many charges are remaining
        public string remainingCharges = "ChargesRemaining";


        public CompProperties_AbilityReloadable()
        {
            compClass = typeof(CompAbilityEffect_Reloadable);
        }
    }
}
