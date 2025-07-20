using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class JobGiver_AICastAbilityTargetToCaster : ThinkNode_JobGiver
    {
        private AbilityDef ability;

        public float maxBodySize = 1f; // Limited to human sized targets by default to avoid pulling large animals or mechs

        protected override Job TryGiveJob(Pawn pawn)
        {
            Ability castingAbility = pawn.abilities?.GetAbility(ability);
            if (castingAbility == null || !castingAbility.CanCast.Accepted || castingAbility.comps.NullOrEmpty()) return null;

            float range = 999f;
            bool flag = false;

            foreach (AbilityComp abilityComp in castingAbility.comps)
                if (abilityComp is CompAbilityEffect_WithDest destComp)
                {
                    if (destComp.Props.destination == AbilityEffectDestination.RandomInRange || destComp.Props.range == 0)
                    {
                        // Randomness will not be accepted because this is supposed to be a consistent method of getting the target to the caster
                        flag = false;
                        break;
                    }
                    if (destComp.Props.range < range) range = destComp.Props.range; // Working on finding the lowest viable range
                    flag = true;
                }

            if (!flag) return null;
            Thing enemy = pawn.GetCurrentTarget(autoSearch: true, searchRadius: range);
            // This is making the assumption that if the current/randomly picked enemy is too large, then you don't want to pull any of the targets next to you
            if (enemy == null || !(enemy is Pawn target) || target.BodySize > maxBodySize || !castingAbility.CanApplyOn(new LocalTargetInfo(target))) return null;
            return castingAbility.GetJob(target, pawn.Position);
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AICastAbilityTargetToCaster obj = (JobGiver_AICastAbilityTargetToCaster)base.DeepCopy(resolve);
            obj.ability = ability;
            return obj;
        }
    }
}
