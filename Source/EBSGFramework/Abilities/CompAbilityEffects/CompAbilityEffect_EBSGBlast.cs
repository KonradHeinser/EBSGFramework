using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;
using UnityEngine;

namespace EBSGFramework
{
    public class CompAbilityEffect_EBSGBlast : CompAbilityEffect
    {
        public new CompProperties_EBSGBlast Props => (CompProperties_EBSGBlast)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<Thing> ignoreList = new List<Thing>();
            Pawn caster = parent.pawn;
            float radius = Props.radius;
            if (Props.statRadius != null && caster.GetStatValue(Props.statRadius) > 0) radius = caster.GetStatValue(Props.statRadius);

            Faction faction;
            if (caster.Dead) faction = caster.Corpse.Faction;
            else faction = caster.Faction;

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

            if ((int)Props.extraGasType != 1)
            {
                GenExplosion.DoExplosion(target.Cell, caster.Map, radius, Props.damageDef, caster, Props.damageAmount,
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, (GasType)(int)Props.extraGasType, Props.applyDamageToExplosionCellsNeighbors,
                    Props.preExplosionThing, Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire,
                    Props.damageFalloff, null, ignoreList, null, true, 1f, Props.excludeRadius, true,
                    Props.postExplosionThingWater, Props.screenShakeFactor);
            }
            else
            {
                GenExplosion.DoExplosion(target.Cell, caster.Map, radius, Props.damageDef, caster, Props.damageAmount,
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, null, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionThing,
                    Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, ignoreList,
                    null, true, 1f, Props.excludeRadius, true, Props.postExplosionThingWater, Props.screenShakeFactor);
            }
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(EBSGUtilities.AffectedCells(target, parent.pawn.Map, parent.pawn, Props.radius).ToList(), Valid(target) ? Color.white : Color.red);
        }
    }
}
