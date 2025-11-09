using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_AICastAnyOfAbilityOnEnemyTarget : JobGiver_AICastAbility
    {
        private List<AbilityDef> abilities = null;
        private int hashInterval = 10; // Alters the chances of the pawn actually trying to cast the ability. If this is set to 1, then the pawn will always attempt to use this, thus making it more difficult to use other abilties. Only recommended for abilities that should be constantly used, like attacks
        List<Ability> presentAbilities = new List<Ability>();
        static Random rnd = new Random();
        Thing currentEnemy;
        Ability chosenAbility;

        protected override Job TryGiveJob(Pawn pawn) // Change this to select its own target instead of using the pawn's current one
        {
            if (!pawn.IsHashIntervalTick(hashInterval)) return null;
            currentEnemy = pawn.GetCurrentTarget(autoSearch: true);
            if (currentEnemy == null || !currentEnemy.HostileTo(pawn)) return null;

            presentAbilities.Clear();
            chosenAbility = null;

            IntVec3 enemyPosition = currentEnemy.Position;
            bool los = GenSight.LineOfSight(pawn.Position, enemyPosition, pawn.Map);
            foreach (AbilityDef abilityDef in abilities)
            {
                if (pawn.CurJobDef == abilityDef.jobDef) return null;
                Ability tempAbility = pawn.abilities?.GetAbility(abilityDef);
                if (tempAbility != null && tempAbility.CanCast)
                {
                    if (tempAbility.CompOfType<CompAbilityEffect_AutocastToggle>()?.autocast == false)
                        continue;
                    if (!tempAbility.ValidateGlobalTarget(currentEnemy)) continue;
                    if (!tempAbility.Valid(currentEnemy)) continue;
                    if (tempAbility.verb.verbProps.requireLineOfSight && !los) continue;
                    if (tempAbility.def.EffectRadius > 0 && tempAbility.def.targetRequired &&
                        enemyPosition.DistanceTo(pawn.Position) <= tempAbility.def.EffectRadius)
                        continue;

                    if (!tempAbility.def.targetRequired)
                    {
                        if (tempAbility.def.EffectRadius <= 0) continue;
                        if (enemyPosition.DistanceTo(pawn.Position) < tempAbility.def.EffectRadius)
                            presentAbilities.Add(tempAbility);
                    }
                    else
                    {
                        if (enemyPosition.DistanceTo(pawn.Position) < tempAbility.verb.EffectiveRange)
                            presentAbilities.Add(tempAbility);
                    }
                }
            }
            if (presentAbilities.NullOrEmpty()) return null;
            chosenAbility = presentAbilities[rnd.Next(presentAbilities.Count)];
            LocalTargetInfo target = GetTarget(pawn, chosenAbility);
            if (!target.IsValid) return null;
            return chosenAbility.GetJob(target, target);
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            // If it's supposed to be cast on the caster (i.e. a burst) return the caster
            if (!ability.def.targetRequired) return new LocalTargetInfo(caster);
            // If targetting a pawn, but can't cast pawns, just target the ground
            if (currentEnemy is Pawn pawnTarget && !ability.verb.verbProps.targetParams.canTargetPawns)
            {
                return new LocalTargetInfo(currentEnemy.Position);
            }
            if (!ability.CanApplyOn(new LocalTargetInfo(currentEnemy))) return LocalTargetInfo.Invalid;
            return new LocalTargetInfo(currentEnemy);
        }
    }
}
