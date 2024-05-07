using Verse;
using RimWorld;
using UnityEngine;

namespace EBSGFramework
{
    public class CompProperties_OrbitalStrike : CompProperties_AbilityEffect
    {
        public enum Gases
        {
            Smoke = 0,
            None = 1,
            Tox = 8,
            Rot = 24
        }

        public ThingDef centerMarker;
        public SoundDef preImpactSound;
        public int count = 30;
        public int interval = 18;
        public int warmupTicks = 60;
        public int targetRadius = 25; // The area the strikes can randomly land in
        public string projectileTexPath;
        public Color projectileColor = Color.white;

        public DamageDef damageDef;
        public FloatRange explosionRadius = new FloatRange(6f, 8f);
        public int damageAmount = -1;
        public float armorPenetration = -1f;
        public bool damageFalloff = false;
        public float fireChance = 1f;
        public Gases extraGasType = Gases.None; // Converted to int in the comp and to gas type in the orbital strike thing

        public SoundDef explosionSound = null;
        public ThingDef postExplosionThing = null; // This is usually what you want
        public float postExplosionThingChance = 0f;
        public int postExplosionSpawnThingCount = 1;
        public ThingDef preExplosionThing = null;
        public float preExplosionThingChance = 0f;
        public int preExplosionSpawnThingCount = 1;
        public ThingDef postExplosionThingWater = null;
        public float screenShakeFactor = 0;

        public CompProperties_OrbitalStrike()
        {
            compClass = typeof(CompAbilityEffect_OrbitalStrike);
        }
    }
}
