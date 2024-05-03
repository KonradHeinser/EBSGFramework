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

        public DamageDef damage;

        public float fireChance = 1f;

        public SoundDef preImpactSound;

        public int count = 30;

        public int interval = 18;

        public int warmupTicks = 60;

        public FloatRange explosionRadius = new FloatRange(6f, 8f);

        public int targetRadius = 25;

        public Gases extraGasType = Gases.Smoke; // Converted to gas type in comp

        public string projectileTexPath;

        public Color projectileColor = Color.white;

        public CompProperties_OrbitalStrike()
        {
            compClass = typeof(CompAbilityEffect_OrbitalStrike);
        }
    }
}
