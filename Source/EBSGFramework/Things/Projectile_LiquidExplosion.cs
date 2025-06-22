using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class Projectile_LiquidExplosion : Projectile_Liquid
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map; // Stores map here because base.Impact's base.Impact destroys the thing, and thus the map reference
            base.Impact(hitThing, blockedByShield);
            GenExplosion.DoExplosion(Position, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, 0, 0, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing,
                def.projectile.postExplosionSpawnThingDef, def.projectile.postExplosionSpawnChance, def.projectile.postExplosionSpawnThingCount,
                def.projectile.postExplosionGasType, null, 255, def.projectile.applyDamageToExplosionCellsNeighbors, null, 0, 1, def.projectile.explosionChanceToStartFire,
                def.projectile.explosionDamageFalloff, null, null, null, def.projectile.doExplosionVFX, def.projectile.damageDef.expolosionPropagationSpeed, 0, def.projectile.soundExplode != null);
        }
    }
}
