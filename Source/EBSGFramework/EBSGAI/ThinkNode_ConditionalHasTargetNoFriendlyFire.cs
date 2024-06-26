using Verse;
using Verse.AI;
using System.Collections.Generic;
using RimWorld;

namespace EBSGFramework
{
    public class ThinkNode_ConditionalHasTargetNoFriendlyFire : ThinkNode_Conditional
    {
        private float minRadius = -1;
        private int minEnemies = 1;
        private AbilityDef ability = null;
        private bool noTarget = false; // If you don't want to reference a specific ability for whatever reason, this can be used to signify the caster is the target
        private bool avoidSelfHit = true; // For abilities like laser eyes that don't use traditional AoE's, this should be false. Burst types always treat this as false
        protected override bool Satisfied(Pawn pawn)
        {
            if (ability == null && minRadius < 0) return false;

            if (ability != null && !ability.targetRequired)
            {
                avoidSelfHit = false; // Set to false because the ability is always centered on caster
                noTarget = true;
            }

            Thing target = null;
            if (noTarget) target = pawn;
            else
            {
                target = EBSGUtilities.GetCurrentTarget(pawn, autoSearch: true);
                if (target == null || !target.HostileTo(pawn)) return false;
            }

            float safetyRange = 0;
            if (ability != null && ability.EffectRadius > 0)
            {

                if (ability.verbProperties.warmupTime > 5) return false; // There are too many things to try to predict to accept long charge times
                if (ability.verbProperties.warmupTime > 1) safetyRange = ability.EffectRadius * 1.1f * ability.verbProperties.warmupTime; // Factors in warmup time when it may be a significant factor
                else safetyRange = ability.EffectRadius * 1.1f; // The 1.1 is an extra "just to be safe"
            }
            if (minRadius > safetyRange)
            {
                safetyRange = minRadius;
                if (!noTarget && avoidSelfHit && target.Position.DistanceTo(pawn.Position) <= minRadius) return false; // If the caster is too close, don't need to check anything else
            }

            if (avoidSelfHit && !noTarget && target.Position.DistanceTo(pawn.Position) <= safetyRange) return false; // If the caster is too close, don't need to check anything else

            List<Pawn> list = pawn.Map.mapPawns.AllPawns;
            list.SortBy((Pawn c) => c.Position.DistanceToSquared(target.Position));
            int targets = 0;
            foreach (Pawn p in list)
            {
                if (p.Position.DistanceTo(target.Position) > safetyRange) break; // Due to earlier sorting, every pawn beyond this point can be ignored
                if ((!avoidSelfHit && p == pawn) || p.Dead || !p.Spawned) continue;
                if (p.Faction != null && !p.Faction.HostileTo(pawn.Faction)) return false; // If the pawn belongs to a non-hostile faction, treat it as friendly fire
                if (!p.HostileTo(pawn)) return false; // Secondary check for general hostility. This is intended to cover factionless things like animals while hopefully avoiding weird scenarios
                targets++;
            }
            if (targets < minEnemies) return false; // If there aren't enough targets to make the attack worth it, return false anyway
            return true; // If no non-enemies were found, fire away
        }
    }
}
