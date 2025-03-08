using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalHasAbilityUsable : ThinkNode_Conditional
    {
        public AbilityDef ability;

        protected override bool Satisfied(Pawn pawn)
        {
            Ability a = pawn.abilities?.GetAbility(ability);
            if (a == null)
                return false;

            if (a.OnCooldown || a.Casting || (a.UsesCharges && a.RemainingCharges == 0))
                return false;

            if (a.CompOfType<CompAbilityEffect_AutocastToggle>()?.autocast == false)
                return false;

            if (a.CompOfType<CompAbilityEffect_HemogenCost>()?.AICanTargetNow(null) == false)
                return false;

            if (a.CompOfType<CompAbilityEffect_ResourceCost>()?.AICanTargetNow(null) == false)
                return false;

            if (a.CompOfType<CompAbilityEffect_EnergyBurst>()?.AICanTargetNow(null) == false)
                return false;

            if (a.CompOfType<CompAbilityEffect_EnergyBlast>()?.AICanTargetNow(null) == false)
                return false;

            if (a.CompOfType<CompAbilityEffect_ConvertResource>()?.AICanTargetNow(null) == false)
                return false;

            return true;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNode_ConditionalHasAbilityUsable obj = (ThinkNode_ConditionalHasAbilityUsable)base.DeepCopy(resolve);
            obj.ability = ability;
            return obj;
        }
    }
}
