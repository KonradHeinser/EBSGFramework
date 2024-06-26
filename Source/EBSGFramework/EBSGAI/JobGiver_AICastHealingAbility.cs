using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimWorld;
using System.Linq;

namespace EBSGFramework
{
    public class JobGiver_AICastHealingAbility : JobGiver_AICastAbility
    {
        private float bleedThreshold = 0.1f;

        private bool allTendable = true;

        private Pawn targetPawn = null;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.CurJobDef == this.ability.jobDef)
                return null;

            Ability castingAbility = pawn.abilities?.GetAbility(this.ability);
            if (ability == null || !castingAbility.CanCast)
                return null;

            LocalTargetInfo target = GetTarget(pawn, castingAbility);
            if (!target.IsValid)
                return null;

            if (EBSGUtilities.NeedToMove(castingAbility, pawn, targetPawn)) return EBSGUtilities.GoToTarget(target); // Check if you need to move closer

            // If close enough, just cast the ability
            if (!ability.targetRequired) return castingAbility.GetJob(pawn, new LocalTargetInfo(pawn)); // For AoE's always centered on caster
            return castingAbility.GetJob(targetPawn, target); // Abilities not intended for only the caster
        }

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            List<Pawn> allies = caster.Map.mapPawns.SpawnedPawnsInFaction(caster.Faction);

            if (!allies.NullOrEmpty())
            {
                allies.SortBy((Pawn p) => p.Position.DistanceToSquared(caster.Position));
                foreach (Pawn ally in allies) // Prioritizes bleeding pawns
                {
                    if (ally.health.hediffSet.BleedRateTotal > bleedThreshold && ability.CanApplyOn(new LocalTargetInfo(ally)))
                    {
                        targetPawn = ally;
                        return new LocalTargetInfo(ally);
                    }
                }
                if (allTendable) // If there's no notable bleeding but allowed to heal all wounds, look for any tendable pawn
                {
                    foreach (Pawn ally in allies) // Start with injuries as those are most likely to cause immediate issues
                    {
                        if (!ability.CanApplyOn(new LocalTargetInfo(ally))) continue;
                        if (!ally.health.hediffSet.GetHediffsTendable().Where((Hediff h) => h.BleedRate > 0).ToList().NullOrEmpty())
                        {
                            targetPawn = ally;
                            return new LocalTargetInfo(ally);
                        }
                    }
                }
            }
            return LocalTargetInfo.Invalid;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AICastHealingAbility obj = (JobGiver_AICastHealingAbility)base.DeepCopy(resolve);
            obj.ability = ability;
            obj.bleedThreshold = bleedThreshold;
            obj.allTendable = allTendable;
            return obj;
        }
    }
}
