using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityAutocastToggle : CompProperties_AbilityEffect
    {
        public string active = "EBSG/AutoOn";

        public string inactive = "EBSG/AutoOff";

        public CompProperties_AbilityAutocastToggle() 
        { 
            compClass = typeof(CompAbilityEffect_AutocastToggle);
        }
    }
}
