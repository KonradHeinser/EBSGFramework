using Verse;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using UnityEngine;

namespace EBSGFramework
{
    public class BurstHediffCompBase : HediffComp
    {
        public BurstHediffPropertiesBase Props => (BurstHediffPropertiesBase)props;

        private float DamageFactor
        {
            get
            {
                if (Props.multiplyDamageBySeverity)
                    return parent.Severity;
                return 1f;
            }
        }

        public void DoExplosion(IntVec3 center)
        {
            if (Props.validSeverities.min != Props.validSeverities.max)
            {
                if (!Props.validSeverities.Includes(parent.Severity)) return;
            }
            else if (parent.Severity < Props.validSeverities.min) return;
            List<Thing> ignoreList = new List<Thing>();
            Pawn caster = parent.pawn;

            Map map = caster.Corpse.MapHeld;
            if (map == null) return;

            float radius = Props.radius;
            if (Props.statRadius != null && caster.GetStatValue(Props.statRadius) > 0) radius = caster.GetStatValue(Props.statRadius);
            if (Props.multiplyRadiusBySeverity)
                radius *= parent.Severity;

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


            if ((int)Props.extraGasType != 1)
            {
                GenExplosion.DoExplosion(center, map, radius, Props.damageDef, caster, Mathf.CeilToInt(Props.damageAmount * DamageFactor),
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, (GasType)(int)Props.extraGasType, Props.applyDamageToExplosionCellsNeighbors,
                     Props.preExplosionThing, Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire,
                     Props.damageFalloff, null, ignoreList, null, true, 1f, Props.excludeRadius, true,
                     Props.postExplosionThingWater, Props.screenShakeFactor);
            }
            else
            {
                GenExplosion.DoExplosion(center, map, radius, Props.damageDef, caster, Mathf.CeilToInt(Props.damageAmount * DamageFactor),
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, null, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionThing,
                    Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, ignoreList,
                    null, true, 1f, Props.excludeRadius, true, Props.postExplosionThingWater, Props.screenShakeFactor);
            }
        }
    }
}
