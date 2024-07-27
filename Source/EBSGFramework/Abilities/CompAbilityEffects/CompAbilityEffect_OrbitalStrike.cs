using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class CompAbilityEffect_OrbitalStrike : CompAbilityEffect
    {
        public new CompProperties_OrbitalStrike Props => (CompProperties_OrbitalStrike)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Thing thing = GenSpawn.Spawn(Props.centerMarker, target.Cell, parent.pawn.Map);

            if (thing is CustomizeableOrbitalStrike obj)
            {
                obj.explosionCount = Props.count;
                obj.bombIntervalTicks = Props.interval;
                obj.warmupTicks = Props.warmupTicks;
                obj.explosionRadiusRange = Props.explosionRadius;
                obj.randomFireRadius = Props.targetRadius;

                if (Props.preImpactSound != null)
                    obj.preImpactSound = Props.preImpactSound;
                if (Props.projectileTexPath != null)
                    obj.projectileTexPath = Props.projectileTexPath;
                obj.projectileColor = Props.projectileColor;
                obj.instigator = parent.pawn;

                if (Props.damageDef != null)
                {
                    obj.damage = Props.damageDef;
                    obj.damageDef = Props.damageDef.defName;
                }
                else
                {
                    obj.damage = DamageDefOf.Bomb;
                    obj.damageDef = "Bomb";
                }

                if (Props.explosionSound != null)
                {
                    obj.explosionSound = Props.explosionSound;
                    obj.explosionSoundDef = Props.explosionSound.defName;
                }
                else
                {
                    obj.explosionSound = null;
                    obj.explosionSoundDef = null;
                }

                obj.damageAmount = Props.damageAmount;
                obj.armorPenetration = Props.armorPenetration;
                obj.damageFalloff = Props.damageFalloff;
                obj.impactAreaRadius = Props.explosionRadius.Average * 2;
                obj.extraGasType = (int)Props.extraGasType;
                obj.fireChance = Props.fireChance;
                obj.screenShakeFactor = Props.screenShakeFactor;

                if (Props.preExplosionThing != null)
                {
                    obj.preExplosionThing = Props.preExplosionThing;
                    obj.preExplosionThingDef = Props.preExplosionThing.defName;
                    obj.preExplosionThingChance = Props.preExplosionThingChance;
                    obj.preExplosionSpawnThingCount = Props.preExplosionSpawnThingCount;
                }
                else
                {
                    obj.preExplosionThing = null;
                    obj.preExplosionThingDef = null;
                }

                if (Props.postExplosionThing != null)
                {
                    obj.postExplosionThing = Props.postExplosionThing;
                    obj.postExplosionThingDef = Props.postExplosionThing.defName;
                    obj.postExplosionThingChance = Props.postExplosionThingChance;
                    obj.postExplosionSpawnThingCount = Props.postExplosionSpawnThingCount;
                }
                else
                {
                    obj.postExplosionThing = null;
                    obj.postExplosionThingDef = null;
                }

                if (Props.postExplosionThingWater != null)
                {
                    obj.postExplosionThingWater = Props.postExplosionThingWater;
                    obj.postExplosionThingWaterDef = Props.postExplosionThingWater.defName;
                }
                else
                {
                    obj.postExplosionThingWater = null;
                    obj.postExplosionThingWaterDef = null;
                }

                obj.StartStrike();
            }
            else
                Log.Error(parent.def.defName + " is using a centerMarker that doesn't use the EBSGFramework.CustomizeableOrbitalStrike thingClass.");
        }
    }
}
