using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System;

namespace EBSGFramework
{
    public class JobGiver_AICastAnyOfAbilityOnEnemyTarget : JobGiver_AICastAbility
    {
        private List<AbilityDef> abilities = null;
        private int hashInterval = 10; // Alters the chances of the pawn actually trying to cast the ability. If this is set to 1, then the pawn will always attempt to use this, thus making it more difficult to use other abilties. Only recommended for abilities that should be constantly used, like attacks
        List<Ability> presentAbilities = new List<Ability>();
        static Random rnd = new Random();
        Thing currentEnemy = null;
        Ability chosenAbility = null;

        protected override Job TryGiveJob(Pawn pawn) // Change this to select its own target instead of using the pawn's current one
        {
            if (!pawn.IsHashIntervalTick(hashInterval)) return null;
            currentEnemy = EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true);
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
                    if (!tempAbility.ValidateGlobalTarget(currentEnemy)) continue;
                    if (tempAbility.verb.verbProps.requireLineOfSight && !los) continue;
                    bool flag = false;
                    if (currentEnemy is Pawn otherPawn && !tempAbility.comps.NullOrEmpty())
                    {
                        foreach (AbilityComp compAbility in tempAbility.comps)
                        {
                            if (compAbility is CompAbilityEffect_GiveHediff comp)
                            {
                                if (comp.Props.psychic && otherPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0) flag = true;
                                else if (comp.Props.durationMultiplier != null && otherPawn.GetStatValue(comp.Props.durationMultiplier) <= 0) flag = true;
                                else if (EBSGUtilities.HasHediff(otherPawn, comp.Props.hediffDef))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            else if (compAbility is CompAbilityEffect_GiveMultipleHediffs multiComp)
                            {
                                if (multiComp.Props.psychic && otherPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0) flag = true;
                                else if (multiComp.Props.durationMultiplier != null && otherPawn.GetStatValue(multiComp.Props.durationMultiplier) <= 0) flag = true;
                                else
                                {
                                    foreach (HediffToGive hediff in multiComp.Props.hediffsToGive)
                                    {
                                        if (hediff.psychic && otherPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        if (EBSGUtilities.HasHediff(otherPawn, hediff.hediffDef))
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                                if (flag) break;
                            }
                            else if (compAbility is CompAbilityEffect_BloodDrain bloodComp)
                            {
                                if (bloodComp.Props.psychic && otherPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0) flag = true;
                                else if (bloodComp.Props.replacementHediff != null && EBSGUtilities.HasHediff(otherPawn, bloodComp.Props.replacementHediff))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            else if (compAbility is CompAbilityEffect_Stun stunComp)
                            {
                                if (stunComp.Props.psychic && otherPawn.GetStatValue(StatDefOf.PsychicSensitivity) <= 0) flag = true;
                                else if (stunComp.Props.durationMultiplier != null && otherPawn.GetStatValue(stunComp.Props.durationMultiplier) <= 0) flag = true;
                                else if (tempAbility.lastCastTick >= 0 && tempAbility.def.EffectDuration() > 0)
                                {
                                    if (Find.TickManager.TicksGame - tempAbility.lastCastTick < tempAbility.def.EffectDuration())
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (flag) continue;
                    if (tempAbility.verb.verbProps.rangeStat != null)
                    {
                        if (enemyPosition.DistanceTo(pawn.Position) < pawn.GetStatValue(tempAbility.verb.verbProps.rangeStat))
                        {
                            presentAbilities.Add(tempAbility);
                        }
                    }
                    else if (!tempAbility.def.targetRequired && tempAbility.def.EffectRadius > 0)
                    {
                        if (enemyPosition.DistanceTo(pawn.Position) < tempAbility.def.EffectRadius)
                        {
                            presentAbilities.Add(tempAbility);
                        }
                    }
                    else
                    {
                        if (enemyPosition.DistanceTo(pawn.Position) < tempAbility.verb.verbProps.range)
                        {
                            presentAbilities.Add(tempAbility);
                        }
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
                return new LocalTargetInfo(currentEnemy);
            }
            if (!ability.CanApplyOn(new LocalTargetInfo(currentEnemy))) return LocalTargetInfo.Invalid;
            return new LocalTargetInfo(currentEnemy);
        }
    }
}
