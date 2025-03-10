using Verse;
using RimWorld;

namespace EBSGFramework
{
    public class BurstHediffPropertiesBase : HediffCompProperties
    {
        public float radius;
        public StatDef statRadius;
        public bool multiplyRadiusBySeverity = false;
        public DamageDef damageDef;
        public int damageAmount = -1;
        public bool multiplyDamageBySeverity = false;

        public float armorPenetration = -1f;
        public bool damageFalloff = false;
        public Gases extraGasType = Gases.Smoke; // Converted to gas type in comp
        public bool applyDamageToExplosionCellsNeighbors = false; // Should probably stay this way
        public float chanceToStartFire = 0f;
        public float excludeRadius = 0f; // Usability is questionable

        public bool injureSelf = false;
        public bool injureAllies = true;
        public bool injureNonHostiles = true;
        public ExclusionLevel? exclusions;

        public SoundDef explosionSound = null;
        public ThingDef postExplosionThing = null; // This is usually what you want
        public float postExplosionThingChance = 0f;
        public int postExplosionSpawnThingCount = 1;
        public ThingDef preExplosionThing = null;
        public float preExplosionThingChance = 0f;
        public int preExplosionSpawnThingCount = 1;
        public ThingDef postExplosionThingWater = null;
        public float screenShakeFactor = 0;

        public float minSeverity = 0f;
        public float maxSeverity = float.MaxValue;
        public FloatRange validSeverities = new FloatRange(0, float.MaxValue);
    }
}
