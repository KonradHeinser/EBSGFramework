using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_AbilityChangeGender : CompProperties_AbilityEffect
    {
        public Gender? target = null;
        
        public Gender? caster = null;
        
        public bool keepName = false;
        
        public CompProperties_AbilityChangeGender()
        {
            compClass = typeof(CompAbilityEffect_ChangeGender);
        }
    }
}
