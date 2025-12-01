using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace EBSGFramework
{
    public class JobGiver_AITeleportEscapeEnemies : ThinkNode_JobGiver
    {
        private AbilityDef ability;

        private static List<Thing> tmpHostileSpots = new List<Thing>();

        protected override Job TryGiveJob(Pawn pawn)
        {
            Ability castingAbility = pawn.abilities?.GetAbility(this.ability);
            if (castingAbility == null || !castingAbility.CanCast || castingAbility.comps.NullOrEmpty()) return null;
            float range = 0f;
            CompAbilityEffect_Teleport teleportComp = null;
            foreach (AbilityComp comp in castingAbility.comps)
                if (comp is CompAbilityEffect_Teleport compAbilityEffect_Teleport)
                {
                    teleportComp = compAbilityEffect_Teleport;
                    if (compAbilityEffect_Teleport.Props.range > 0) range = compAbilityEffect_Teleport.Props.range;
                    else range = compAbilityEffect_Teleport.Props.randomRange.RandomInRange;
                    break;
                }

            if (teleportComp == null) return null;
            if (TryFindRelocatePosition(pawn, out var relocatePosition, range, teleportComp))
                return castingAbility.GetJob(pawn, relocatePosition);

            return null;
        }

        private bool TryFindRelocatePosition(Pawn pawn, out IntVec3 relocatePosition, float maxDistance, CompAbilityEffect_Teleport teleportComp)
        {
            tmpHostileSpots.Clear();
            tmpHostileSpots.AddRange(from a in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
                                     where !a.ThreatDisabled(pawn)
                                     select a.Thing);
            Ability jump = pawn.abilities?.GetAbility(ability);
            relocatePosition = CellFinderLoose.GetFallbackDest(pawn, tmpHostileSpots, maxDistance, 5f, 5f, 20, c => teleportComp.Valid(c, false));
            tmpHostileSpots.Clear();
            return relocatePosition.IsValid;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_AITeleportEscapeEnemies obj = (JobGiver_AITeleportEscapeEnemies)base.DeepCopy(resolve);
            obj.ability = ability;
            return obj;
        }
    }
}
