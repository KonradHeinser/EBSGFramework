using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalValidTargetForAbility : ThinkNode_Conditional
    {
        private AbilityDef ability = null;

        private bool onlyHostiles = true;

        private bool onlyInFaction = false;

        private bool autoSearch = true;

        protected override bool Satisfied(Pawn pawn)
        {
            if (ability == null) return false;

            Ability pawnAbility = pawn.abilities?.GetAbility(ability);
            if (pawnAbility == null || !pawnAbility.CanCast || pawnAbility.comps.NullOrEmpty() || !ability.targetRequired) return false;

            Thing target = pawn.GetCurrentTarget(onlyHostiles, onlyInFaction, autoSearch);

            try
            {
                ability.verbProperties.targetParams.CanTarget(target);
                foreach (CompAbilityEffect abilityEffect in pawnAbility.comps)
                    if (!abilityEffect.Valid((LocalTargetInfo)target)) return false;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
