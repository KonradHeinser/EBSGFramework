using Verse;
using Verse.AI;
using RimWorld;

namespace EBSGFramework
{
    public class JobGiver_AICastAbilityGoToTarget : JobGiver_AICastAbility
    {
        private bool safeJumpsOnly = true; // Only jumps at a target if they are within half range to avoid pawns jumping too far ahead of allies
        private bool onlyWhileWieldingMeleeWeapon = true;
        Thing currentEnemy = null;

        protected override Job TryGiveJob(Pawn pawn) // Change this to select its own target instead of using the pawn's current one
        {
            if (onlyWhileWieldingMeleeWeapon && (pawn.equipment.Primary == null || pawn.equipment.Primary.def.IsRangedWeapon)) return null;
            Ability ability = pawn.abilities?.GetAbility(this.ability);
            if (ability == null || !ability.CanCast) return null;
            currentEnemy = EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true, LoSRequired: ability.verb.verbProps.requireLineOfSight);
            if (currentEnemy == null) return null;
            float currentEnemyDistance = currentEnemy.Position.DistanceTo(pawn.Position);
            float range = 0f;

            CompAbilityEffect_Teleport teleportComp = null;
            foreach (AbilityComp comp in ability.comps)
            {
                if (comp is CompAbilityEffect_Teleport compAbilityEffect_Teleport)
                {
                    teleportComp = compAbilityEffect_Teleport;
                    if (teleportComp.Props.destination != AbilityEffectDestination.Selected) return null; // Not built to handle non-targetted casts
                    if (compAbilityEffect_Teleport.Props.range > 0) range = compAbilityEffect_Teleport.Props.range;
                    else range = compAbilityEffect_Teleport.Props.randomRange.RandomInRange;
                    break;
                }
            }
            if (teleportComp == null)
            {
                if (ability.verb.verbProps.rangeStat != null) range = pawn.GetStatValue(ability.verb.verbProps.rangeStat);
                else range = ability.verb.verbProps.range;
            }
            if (safeJumpsOnly) range /= 2;
            if (currentEnemyDistance < 5 || currentEnemyDistance > range) return null;
            LocalTargetInfo destination = GetTarget(pawn, ability);
            if (!destination.IsValid) return null;
            if (teleportComp != null) return ability.GetJob(pawn, destination);
            return ability.GetJob(destination, destination);
        }
        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            // If targetting a pawn, but can't cast pawns, just target the ground
            if (currentEnemy is Pawn pawnTarget && !ability.verb.verbProps.targetParams.canTargetPawns)
            {
                return new LocalTargetInfo(currentEnemy);
            }
            if (!ability.CanApplyOn(new LocalTargetInfo(currentEnemy))) return LocalTargetInfo.Invalid;
            return new LocalTargetInfo(currentEnemy);
        }
    }
}
