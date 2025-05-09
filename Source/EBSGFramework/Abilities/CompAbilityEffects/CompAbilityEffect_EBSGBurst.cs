﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;

namespace EBSGFramework
{
    public class CompAbilityEffect_EBSGBurst : CompAbilityEffect
    {
        public new CompProperties_EBSGBurst Props => (CompProperties_EBSGBurst)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> ignoreList = new List<Thing>();
            Pawn caster = parent.pawn;

            if (Props.effecter != null)
            {
                Effecter effecter = Props.effecter.Spawn();
                if (Props.effecterTicks != 0)
                {
                    caster.Map.effecterMaintainer.AddEffecterToMaintain(effecter, caster.Position, Props.effecterTicks);
                }
                else
                {
                    effecter.Trigger(new TargetInfo(caster.Position, caster.Map), new TargetInfo(caster.Position, caster.Map));
                    effecter.Cleanup();
                }
            }

            float radius = Props.radius;
            if (Props.statRadius != null)
            {
                if (caster.GetStatValue(Props.statRadius) > 0) radius = caster.GetStatValue(Props.statRadius);
                else radius = 0;
            }

            Faction faction;
            if (caster.Dead) faction = caster.Corpse.Faction;
            else faction = caster.Faction;

            if (Props.exclusions != null)
            {
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
            }
            else
            {
                if (!Props.injureNonHostiles)
                {
                    foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned)
                    {
                        if (!caster.Dead)
                        {
                            if (!pawn.HostileTo(caster))
                            {
                                ignoreList.Add(pawn);
                            }
                        }
                        else
                        {
                            if (!pawn.Faction.HostileTo(faction))
                            {
                                ignoreList.Add(pawn);
                            }
                        }

                    }
                }
                else if (!Props.injureAllies)
                {
                    foreach (Pawn pawn in caster.Map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null && p.Faction == faction))
                    {
                        ignoreList.Add(pawn);
                    }
                }
                else if (!Props.injureSelf && !caster.Dead)
                {
                    ignoreList.Add(caster);
                }
            }

            int damageAmount = Props.damageStat != null ? Mathf.FloorToInt(caster.StatOrOne(Props.damageStat)) : Props.damageAmount;

            if ((int)Props.extraGasType != 1)
            {
                GenExplosion.DoExplosion(caster.Position, caster.Map, radius, Props.damageDef, caster, damageAmount,
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, (GasType)(int)Props.extraGasType, Props.applyDamageToExplosionCellsNeighbors,
                    Props.preExplosionThing, Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire,
                    Props.damageFalloff, null, ignoreList, null, true, 1f, Props.excludeRadius, true,
                    Props.postExplosionThingWater, Props.screenShakeFactor);
            }
            else
            {
                GenExplosion.DoExplosion(caster.Position, caster.Map, radius, Props.damageDef, caster, damageAmount,
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, null, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionThing,
                    Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, ignoreList,
                    null, true, 1f, Props.excludeRadius, true, Props.postExplosionThingWater, Props.screenShakeFactor);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            float radius = Props.radius;
            if (Props.statRadius != null && parent.pawn.StatOrOne(Props.statRadius) >= 0) radius = parent.pawn.GetStatValue(Props.statRadius);

            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(parent.pawn, parent.pawn.Map, parent.pawn, radius).ToList(), Valid(target) ? Color.white : Color.red);
        }
    }
}
