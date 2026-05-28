using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class JobGiver_AICastBerserkAbility : JobGiver_AICastAbility
    {
        private static List<Pawn> potentialTargets = new List<Pawn>();

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            potentialTargets.Clear();
            var hostiles = (from x in caster.Map.attackTargetsCache.GetPotentialTargetsFor(caster)
                                          select x.Thing).ToList();
            if (hostiles.NullOrEmpty())
                return LocalTargetInfo.Invalid;

            // Get verbs
            var verb = ability.verb.verbProps;
            var validTargets = verb.targetParams;

            // Get range
            var MaxDistanceFromCaster = ability.verb.EffectiveRange;
            if (ability.def.verbProperties.warmupTime > 1) MaxDistanceFromCaster *= (1 / ability.def.verbProperties.warmupTime) * 0.8f; // Trying to avoid chance of target wandering out of range mid-cast

            // Create curve based on range
            var DistanceSquaredToTargetSelectionWeightCurve = new SimpleCurve
            {
                new CurvePoint(MaxDistanceFromCaster * 0.16f, 1f),
                new CurvePoint(MaxDistanceFromCaster * 0.65f, 0.1f),
                new CurvePoint(MaxDistanceFromCaster, 0f)
            };

            // Get Comp
            var mentalComp = ability.comps.FirstOrDefault(c => c is CompAbilityEffect_GiveMentalState) as CompAbilityEffect_GiveMentalState;
            var mentalState = mentalComp?.Props.stateDef;

            if (mentalComp == null) return LocalTargetInfo.Invalid;

            foreach (var item in caster.Map.mapPawns.AllPawnsSpawned)
            {
                // Check if it's even a valid target
                if (!item.Position.InHorDistOf(caster.Position, MaxDistanceFromCaster) || !mentalComp.Valid(new LocalTargetInfo(item)) || !ability.CanApplyOn(new LocalTargetInfo(item))) continue;
                if (item.MentalStateDef == mentalState) continue;

                if (validTargets.CanTarget(item)) potentialTargets.Add(item);
            }
            if (potentialTargets.TryRandomElementByWeight(delegate (Pawn x)
            {
                var num = MaxDistanceFromCaster;
                foreach (var num2 in hostiles.Where(i => i.Spawned).Select(i => i.Position.DistanceTo(x.Position)))
                    if (num2 < num)
                        num = num2;
                return DistanceSquaredToTargetSelectionWeightCurve.Evaluate(num);
            }, out var result))
            {
                return new LocalTargetInfo(result);
            }
            return LocalTargetInfo.Invalid;
        }
    }
}
