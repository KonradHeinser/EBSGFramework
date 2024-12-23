using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_EquippableAbility : CompProperties
    {
        public AbilityDef ability;

        public bool saveCooldown = true; // Technically it's saved anyway, but it's only used if this is false

        public CompProperties_EquippableAbility()
        {
            compClass = typeof(CompEquippableAbility);
        }
    }
}
