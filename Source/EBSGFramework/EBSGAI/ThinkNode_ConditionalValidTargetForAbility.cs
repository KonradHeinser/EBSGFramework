using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalValidTargetForAbility : ThinkNode_Conditional
    {
        private AbilityDef ability = null;

        protected override bool Satisfied(Pawn pawn)
        {
            if (ability == null) return false;

            Ability pawnAbility = pawn.abilities?.GetAbility(ability);
            if (pawnAbility == null || !pawnAbility.CanCast || pawnAbility.comps.NullOrEmpty()) return false;

            Thing target = EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true);

            try
            {
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
