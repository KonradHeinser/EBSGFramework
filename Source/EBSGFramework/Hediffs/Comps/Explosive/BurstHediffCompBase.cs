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

            Map map = Pawn.MapHeld;
            if (map == null) return;

            float radius = Props.radius;
            if (Props.statRadius != null && Pawn.GetStatValue(Props.statRadius) > 0) radius = Pawn.GetStatValue(Props.statRadius);
            if (Props.multiplyRadiusBySeverity)
                radius *= parent.Severity;

            Faction faction;
            if (Pawn.Dead) faction = Pawn.Corpse.Faction;
            else faction = Pawn.Faction;

            if (Props.exclusions != null)
            {
                switch (Props.exclusions)
                {
                    case ExclusionLevel.Self:
                        if (!Pawn.Dead)
                            ignoreList.Add(Pawn);
                        break;
                    case ExclusionLevel.Allies:
                        foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null && p.Faction == faction))
                            ignoreList.Add(pawn);
                        break;
                    case ExclusionLevel.NonHostiles:
                        foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                            if (!Pawn.Dead)
                            {
                                if (!pawn.HostileTo(Pawn))
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
                    foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                    {
                        if (!Pawn.Dead)
                        {
                            if (!pawn.HostileTo(Pawn))
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
                    foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null && p.Faction == faction))
                    {
                        ignoreList.Add(pawn);
                    }
                }
                else if (!Props.injureSelf && !Pawn.Dead)
                {
                    ignoreList.Add(Pawn);
                }
            }


            if ((int)Props.extraGasType != 1)
            {
                GenExplosion.DoExplosion(center, map, radius, Props.damageDef, null, Mathf.CeilToInt(Props.damageAmount * DamageFactor),
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, (GasType)(int)Props.extraGasType, Props.applyDamageToExplosionCellsNeighbors,
                     Props.preExplosionThing, Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire,
                     Props.damageFalloff, null, ignoreList, null, true, 1f, Props.excludeRadius, true,
                     Props.postExplosionThingWater, Props.screenShakeFactor);
            }
            else
            {
                GenExplosion.DoExplosion(center, map, radius, Props.damageDef, null, Mathf.CeilToInt(Props.damageAmount * DamageFactor),
                    Props.armorPenetration, Props.explosionSound, null, null, null, Props.postExplosionThing, Props.postExplosionThingChance,
                    Props.postExplosionSpawnThingCount, null, Props.applyDamageToExplosionCellsNeighbors, Props.preExplosionThing,
                    Props.preExplosionThingChance, Props.preExplosionSpawnThingCount, Props.chanceToStartFire, Props.damageFalloff, null, ignoreList,
                    null, true, 1f, Props.excludeRadius, true, Props.postExplosionThingWater, Props.screenShakeFactor);
            }
        }
    }
}
