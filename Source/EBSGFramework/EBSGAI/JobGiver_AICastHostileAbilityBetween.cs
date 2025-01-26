using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace EBSGFramework
{
    public class JobGiver_AICastHostileAbilityBetween : ThinkNode_JobGiver
    {
        private AbilityDef ability;

        private float percentage = 0.5f;

        public float radius = 0;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (percentage >= 1 || percentage <= 0)
            {
                Log.Error($"{percentage} is out of the valid range. To cast between targets, pick a float between 0 and 1 exclusive.");
                return null;
            }
            Ability cast = pawn.abilities?.GetAbility(ability);
            if (cast?.def.verbProperties?.targetParams?.canTargetLocations != true) return null;
            Thing currentEnemy = pawn.GetCurrentTarget(autoSearch: true, searchRadius: radius == 0 ? cast.verb.EffectiveRange : radius);
            if (currentEnemy == null || !currentEnemy.HostileTo(pawn)) return null;
            IntVec3 enemyPosition = currentEnemy.PositionHeld;
            var availableSpots = GenSight.BresenhamCellsBetween(pawn.PositionHeld, enemyPosition);
            IntVec3 target = availableSpots[Mathf.CeilToInt((availableSpots.Count - 1) * percentage)];
            if (!cast.Valid(target)) return null;
            return cast.GetJob(target, target);
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            var obj = (JobGiver_AICastHostileAbilityBetween)base.DeepCopy(resolve);
            obj.ability = ability;
            obj.percentage = percentage;
            return obj;
        }
    }
}
