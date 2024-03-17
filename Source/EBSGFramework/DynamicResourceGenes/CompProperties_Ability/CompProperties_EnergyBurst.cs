using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_EnergyBurst : CompProperties_AbilityEffect
    {
        public enum Gases
        {
            Smoke = 0,
            None = 1,
            Tox = 8,
            Rot = 24
        }
        public float radiusFactor = 0.1f;
        public StatDef radiusFactorStat;
        public DamageDef damageDef;
        public float damageFactor = 0.1f;
        public float armorPenetration = -1f;
        public SoundDef explosionSound = null;
        public ThingDef postExplosionThing = null; // This is usually what you want
        public float postExplosionThingChance = 0f;
        public int postExplosionSpawnThingCount = 1;
        public Gases extraGasType = Gases.None; // Converted to gas type in comp
        public bool applyDamageToExplosionCellsNeighbors = false; // Should probably stay this way
        public ThingDef preExplosionThing = null; // This is usually what you want
        public float preExplosionThingChance = 0f;
        public bool damageFalloff = true;
        public int preExplosionSpawnThingCount = 1;
        public float chanceToStartFire = 0f;
        public float excludeRadius = 0f; // Usability is questionable
        public ThingDef postExplosionThingWater = null;
        public float screenShakeFactor = 0;
        public bool injureSelf = false;
        public bool injureAllies = true;
        public bool injureNonHostiles = true;

        public float resourceCost = 0; // Amount taken beforehand that alters how much the converted percentage can take, and doesn't directly change the radius or damage
        public float convertedResource = 0; // Flat amount used. Taken after percentage if applicable, though they probably shouldn't be used together
        public float convertPercentage = 0;
        public GeneDef mainResourceGene;
        public StatDef costFactorStat;

        public CompProperties_EnergyBurst()
        {
            compClass = typeof(CompAbilityEffect_EnergyBurst);
        }
    }
}
