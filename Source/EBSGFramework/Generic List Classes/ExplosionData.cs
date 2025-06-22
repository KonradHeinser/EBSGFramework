using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace EBSGFramework
{
    public class ExplosionData
    {
        public float radius;
        public StatDef statRadius;
        public DamageDef damageDef;
        public int damageAmount = -1;
        public StatDef damageStat;
        public float armorPenetration = -1f;
        public SoundDef explosionSound = null;
        public ThingDef postExplosionThing = null; // This is usually what you want
        public float postExplosionThingChance = 0f;
        public int postExplosionSpawnThingCount = 1;
        public GasType? extraGasType = null; // Converted to gas type below
        public float? gasRadiusOverride = null;
        public int postExplosionGasAmount = 255;
        public bool applyDamageToExplosionCellsNeighbors = false; // Should probably stay this way
        public ThingDef preExplosionThing = null;
        public float preExplosionThingChance = 0f;
        public bool damageFalloff = false;
        public int preExplosionSpawnThingCount = 1;
        public float chanceToStartFire = 0f;
        public float excludeRadius = 0f; // Usability is questionable
        public ThingDef postExplosionThingWater = null;
        public float screenShakeFactor = 0;
        public ExclusionLevel exclusions = ExclusionLevel.Self;
        public EffecterDef effecter;
        public int effecterTicks = 0;
        

        // Hediff stuff
        public bool multiplyRadiusBySeverity = false;
        public bool multiplyDamageBySeverity = false;

        public void DoExplosion(Thing caster, LocalTargetInfo target, Map map, float severity = 1f)
        {
            if (map == null) return;
            List<Thing> ignoreList = new List<Thing>();

            if (effecter != null)
            {
                Effecter e = effecter.Spawn();
                if (effecterTicks != 0)
                {
                    caster.Map.effecterMaintainer.AddEffecterToMaintain(e, target.Cell, effecterTicks);
                }
                else
                {
                    e.Trigger(new TargetInfo(target.Cell, caster.Map), new TargetInfo(target.Cell, caster.Map));
                    e.Cleanup();
                }
            }

            int damage = damageAmount;
            if (damageStat != null)
            {
                if (caster.StatOrOne(damageStat) > 0)
                    damage = Mathf.CeilToInt(caster.StatOrOne(damageStat));
                else
                    damage = 0;
            }
            if (multiplyDamageBySeverity)
                damage = Mathf.CeilToInt(damage * severity);

            float r = radius;
            if (statRadius != null)
            {
                if (caster.StatOrOne(statRadius) > 0) 
                    r = caster.StatOrOne(statRadius);
                else 
                    r = 0;
            }
            if (multiplyRadiusBySeverity)
                r *= severity;

            Faction faction;
            faction = caster?.Faction;
            
            switch (exclusions)
            {
                case ExclusionLevel.Self:
                    if (caster != null && (!caster.Destroyed || (caster is Pawn p && !p.Dead)))
                        ignoreList.Add(caster);
                    break;
                case ExclusionLevel.Allies:
                    if (faction != null)
                    {
                        var factionPawns = map.mapPawns.PawnsInFaction(faction);
                        if (!factionPawns.NullOrEmpty())
                            ignoreList.AddRange(factionPawns);
                    }
                    break;
                case ExclusionLevel.NonHostiles:
                    foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                        if (caster != null && (!caster.Destroyed || (caster is Pawn pnh && !pnh.Dead)))
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


            GenExplosion.DoExplosion(target.Cell, caster.Map, radius, damageDef, caster, damage,
                armorPenetration, explosionSound, null, null, null, postExplosionThing,
                postExplosionThingChance, postExplosionSpawnThingCount, extraGasType,
                gasRadiusOverride, postExplosionGasAmount, applyDamageToExplosionCellsNeighbors,
                preExplosionThing, preExplosionThingChance, preExplosionSpawnThingCount,
                chanceToStartFire, damageFalloff, null, ignoreList, null, true, 1f, excludeRadius,
                true, postExplosionThingWater, screenShakeFactor);
        }
    }
}
