using RimWorld;

namespace EBSGFramework
{
    public class CompProperties_AbilityChangeFaction : CompProperties_AbilityEffect
    {
        public FactionDef staticFaction;

        public bool useStatic = false;

        public SuccessChance successChance;

        public CompProperties_AbilityChangeFaction()
        {
            compClass = typeof(CompAbilityEffect_ChangeFaction);
        }
    }
}
