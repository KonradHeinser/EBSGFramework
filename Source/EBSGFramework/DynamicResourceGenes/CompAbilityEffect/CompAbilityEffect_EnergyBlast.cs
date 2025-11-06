using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class CompAbilityEffect_EnergyBlast : CompAbilityEffect
    {
        public new CompProperties_EnergyBlast Props => (CompProperties_EnergyBlast)props;
        public List<AbilityComp> comps;
        ResourceGene cachedResourceGene;

        private float Radius
        {
            get
            {
                if (ResourceGene == null) return 0;
                if (Props.radiusFactorStat != null)
                {
                    if (parent.pawn.GetStatValue(Props.radiusFactorStat) > 0) return parent.pawn.GetStatValue(Props.radiusFactorStat) * ConvertedEnergy;
                    return 0;
                }
                return Props.radiusFactor * ConvertedEnergy;
            }
        }

        private ResourceGene ResourceGene
        {
            get
            {
                if (cachedResourceGene == null)
                {
                    Gene gene = parent.pawn.genes.GetGene(Props.mainResourceGene);
                    if (gene is ResourceGene resourceGene)
                    {
                        cachedResourceGene = resourceGene;
                    }
                }
                return cachedResourceGene;
            }
        }

        public float CurrentCost
        {
            get
            {
                if (ResourceGene == null) return 0;
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);
                float dynamicCost = (ResourceGene.Value - cost) * Props.convertPercentage + Props.convertedResource;

                return cost + dynamicCost;
            }
        }

        private float ConvertedEnergy
        {
            get
            {
                if (ResourceGene == null) return 0;
                float cost = Props.resourceCost;
                if (Props.costFactorStat != null) cost *= parent.pawn.GetStatValue(Props.costFactorStat);

                return ((ResourceGene.Value - cost) * Props.convertPercentage + Props.convertedResource) * 100;
            }
        }

        private bool HasEnoughResource
        {
            get
            {
                if (ResourceGene == null || ResourceGene.Value < CurrentCost || ResourceGene.Value <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            if (Props.mainResourceGene == null) Log.Error(parent.def + " is missing a designated mainResourceGene, meaning it can't alter the resource levels");
            else
            {
                List<Thing> ignoreList = new List<Thing>();
                Faction faction;
                if (caster.Dead) faction = caster.Corpse.Faction;
                else faction = caster.Faction;

                switch (Props.exclusions)
                {
                    case ExclusionLevel.Self:
                        if (!caster.Dead)
                            ignoreList.Add(caster);
                        break;
                    case ExclusionLevel.Allies:
                        foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null && p.Faction == faction))
                            ignoreList.Add(pawn);
                        break;
                    case ExclusionLevel.NonHostiles:
                        foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned)
                            if (!caster.Dead)
                            {
                                if (!pawn.HostileTo(caster))
                                    ignoreList.Add(pawn);
                            }
                            else
                            {
                                if (!pawn.Faction.HostileTo(faction))
                                    ignoreList.Add(pawn);
                            }
                        break;
                }

                GenExplosion.DoExplosion(target.Cell, caster.Map, Radius, Props.damageDef, caster,
                    Mathf.CeilToInt(Props.damageFactor * ConvertedEnergy), Props.armorPenetration, 
                    Props.explosionSound, null, null, null, Props.postExplosionThing, 
                    Props.postExplosionThingChance, Props.postExplosionSpawnThingCount, 
                    Props.extraGasType, Props.gasRadiusOverride, Props.postExplosionGasAmount,
                    Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionThing, 
                    Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, 
                    Props.chanceToStartFire, Props.damageFalloff, null, ignoreList, null, true, 1f, 
                    Props.excludeRadius, true, Props.postExplosionThingWater, Props.screenShakeFactor);

                ResourceGene.OffsetResource(caster, 0f - CurrentCost, ResourceGene);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(target, parent.pawn.Map, parent.pawn, Radius).ToList(), Valid(target) ? Color.white : Color.red);
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (!parent.pawn.HasRelatedGene(Props.mainResourceGene))
            {
                reason = "AbilityDisabledNoResourceGene".Translate(parent.pawn, Props.mainResourceGene.LabelCap);
                return true;
            }

            if (ResourceGene.Value < CurrentCost || ResourceGene.Value <= 0)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, ResourceGene.ResourceLabel);
                return true;
            }
            float num = TotalResourceCostOfQueuedAbilities();
            float num2 = CurrentCost + num;
            if (CurrentCost > float.Epsilon && num2 > ResourceGene.Value)
            {
                reason = "AbilityDisabledNoResource".Translate(parent.pawn, ResourceGene.ResourceLabel);
                return true;
            }
            reason = null;
            return false;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return HasEnoughResource;
        }

        private float TotalResourceCostOfQueuedAbilities()
        {
            float num = ((!(parent.pawn.jobs?.curJob?.verbToUse is Verb_CastAbility verb_CastAbility)) ? 0f : ResourceCost(verb_CastAbility));
            if (parent.pawn.jobs != null)
            {
                for (int i = 0; i < parent.pawn.jobs.jobQueue.Count; i++)
                {
                    if (parent.pawn.jobs.jobQueue[i].job.verbToUse is Verb_CastAbility verb_CastAbility2)
                    {
                        num += ResourceCost(verb_CastAbility2);
                    }
                }
            }
            return num;
        }

        public float ResourceCost(Verb_CastAbility verb_CastAbility2)
        {
            if (comps != null)
            {
                foreach (AbilityComp comp in verb_CastAbility2.ability?.comps)
                {
                    if (comp is CompAbilityEffect_ResourceCost compAbilityEffect_ResourceCost)
                    {
                        return CurrentCost;
                    }
                }
            }
            return 0f;
        }
    }
}
