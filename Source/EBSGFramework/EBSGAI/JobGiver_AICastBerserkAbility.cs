using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EBSGFramework
{
    public class JobGiver_AICastBerserkAbility : JobGiver_AICastAbility
    {
        private static List<Pawn> potentialTargets = new List<Pawn>();

        protected override LocalTargetInfo GetTarget(Pawn caster, Ability ability)
        {
            potentialTargets.Clear();
            IEnumerable<Thing> hostiles = from x in caster.Map.attackTargetsCache.GetPotentialTargetsFor(caster)
                                          select x.Thing;
            if (hostiles.EnumerableNullOrEmpty())
            {
                return LocalTargetInfo.Invalid;
            }

            // Get verbs
            VerbProperties verb = ability.verb.verbProps;
            TargetingParameters validTargets = verb.targetParams;

            // Get range
            float MaxDistanceFromCaster = 0;
            if (verb.rangeStat != null) MaxDistanceFromCaster = caster.GetStatValue(verb.rangeStat);
            else MaxDistanceFromCaster = verb.range;
            if (ability.def.verbProperties.warmupTime > 1) MaxDistanceFromCaster *= (1 / ability.def.verbProperties.warmupTime) * 0.8f; // Trying to avoid chance of target wandering out of range mid-cast

            // Create curve based on range
            SimpleCurve DistanceSquaredToTargetSelectionWeightCurve = new SimpleCurve
            {
                new CurvePoint(MaxDistanceFromCaster * 0.16f, 1f),
                new CurvePoint(MaxDistanceFromCaster * 0.65f, 0.1f),
                new CurvePoint(MaxDistanceFromCaster, 0f)
            };

            // Get Comp
            CompAbilityEffect_GiveMentalState mentalComp = null;
            MentalStateDef mentalState = null;
            foreach (AbilityComp comp in ability.comps)
            {
                if (comp is CompAbilityEffect_GiveMentalState compAbilityEffect_Mental)
                {
                    mentalComp = compAbilityEffect_Mental;
                    mentalState = compAbilityEffect_Mental.Props.stateDef;
                    break;
                }
            }

            if (mentalComp == null) return LocalTargetInfo.Invalid;

            foreach (Pawn item in caster.Map.mapPawns.AllPawnsSpawned)
            {
                // Check if it's even a valid target
                if (!item.Position.InHorDistOf(caster.Position, MaxDistanceFromCaster) || !mentalComp.Valid(new LocalTargetInfo(item)) || !ability.CanApplyOn(new LocalTargetInfo(item))) continue;
                if (item.MentalStateDef == mentalState) continue;

                if (validTargets.CanTarget(item)) potentialTargets.Add(item);
            }
            if (potentialTargets.TryRandomElementByWeight(delegate (Pawn x)
            {
                float num = MaxDistanceFromCaster;
                foreach (Thing item2 in hostiles)
                {
                    if (item2.Spawned)
                    {
                        float num2 = item2.Position.DistanceTo(x.Position);
                        if (num2 < num)
                        {
                            num = num2;
                        }
                    }
                }
                return DistanceSquaredToTargetSelectionWeightCurve.Evaluate(num);
            }, out var result))
            {
                return new LocalTargetInfo(result);
            }
            return LocalTargetInfo.Invalid;
        }
    }
}
