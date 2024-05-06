using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    [StaticConstructorOnStartup]
    public class CustomizeableOrbitalStrike : OrbitalStrike
    {
        public class OrbitalProjectile : IExposable
        {
            private int lifeTime;

            public IntVec3 targetCell;

            public int LifeTime => lifeTime;

            public OrbitalProjectile()
            {
            }

            public OrbitalProjectile(int lifeTime, IntVec3 targetCell)
            {
                this.lifeTime = lifeTime;
                this.targetCell = targetCell;
            }

            public void Tick()
            {
                lifeTime--;
            }

            public void Draw(Material material)
            {
                if (lifeTime > 0)
                {
                    Vector3 pos = targetCell.ToVector3() + Vector3.forward * Mathf.Lerp(60, 0f, 1f - (float)lifeTime / 60f);
                    pos.z += 1.25f;
                    pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                    Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0f, 180f, 0f), new Vector3(2.5f, 1f, 2.5f));
                    Graphics.DrawMesh(MeshPool.plane10, matrix, material, 0);
                }
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref lifeTime, "lifeTime", 0);
                Scribe_Values.Look(ref targetCell, "targetCell");
            }
        }

        public float impactAreaRadius = 15f;

        public FloatRange explosionRadiusRange = new FloatRange(6f, 8f);

        public SoundDef preImpactSound;

        public int damageAmount = -1;

        public float armorPenetration = -1f;

        public SoundDef explosionSound;

        public string explosionSoundDef;

        public ThingDef postExplosionThing;

        public string postExplosionThingDef;

        public float postExplosionThingChance = 0f;

        public int postExplosionSpawnThingCount = 1;

        public ThingDef preExplosionThing;

        public string preExplosionThingDef;

        public float preExplosionThingChance = 0f;

        public bool damageFalloff;

        public int preExplosionSpawnThingCount = 1;

        public ThingDef postExplosionThingWater;

        public string postExplosionThingWaterDef;

        public float screenShakeFactor = 0;

        public int randomFireRadius = 25;

        public int bombIntervalTicks = 18;

        public int warmupTicks = 60;

        public int explosionCount = 30;

        public int explosionsRemaining;

        public float fireChance = 1f;

        public DamageDef damage;

        public string damageDef;

        public int extraGasType = 1;

        public string projectileTexPath = "Things/Projectile/Bullet_Big";

        public Color projectileColor = Color.white;

        private int ticksToNextEffect;

        private IntVec3 nextExplosionCell = IntVec3.Invalid;

        private List<OrbitalProjectile> projectiles = new List<OrbitalProjectile>();

        public const int EffectiveAreaRadius = 23;

        private const int StartRandomFireEveryTicks = 20;

        private const int EffectDuration = 60;

        public static readonly SimpleCurve DistanceChanceFactor = new SimpleCurve
    {
        new CurvePoint(0f, 1f),
        new CurvePoint(1f, 0.1f)
    };

        public override void SpawnSetup(Map map, bool respawningAfterReload)
        {
            base.SpawnSetup(map, respawningAfterReload);
            if (!respawningAfterReload)
            {
                GetNextExplosionCell();
            }
        }

        public override void StartStrike()
        {
            duration = bombIntervalTicks * (explosionCount + 5); // To ensure the last few have a chance to land
            explosionsRemaining = explosionCount;
            base.StartStrike();
        }

        public override void Tick()
        {
            if (Destroyed || !Spawned)
                return;

            if (warmupTicks > 0)
            {
                warmupTicks--;
                if (warmupTicks == 0)
                    StartStrike();
            }
            else
                base.Tick();

            EffectTick();
        }

        private void EffectTick()
        {
            if (!nextExplosionCell.IsValid)
            {
                ticksToNextEffect = warmupTicks - bombIntervalTicks;
                GetNextExplosionCell();
            }
            ticksToNextEffect--;
            if (ticksToNextEffect <= 0 && TicksLeft >= bombIntervalTicks && explosionsRemaining > 0)
            {
                if (preImpactSound != null)
                    preImpactSound.PlayOneShot(new TargetInfo(nextExplosionCell, Map));
                projectiles.Add(new OrbitalProjectile(60, nextExplosionCell));
                explosionsRemaining--;
                ticksToNextEffect = bombIntervalTicks;
                GetNextExplosionCell();
            }

            List<OrbitalProjectile> spentProjectiles = new List<OrbitalProjectile>();

            if (!projectiles.NullOrEmpty())
                foreach (OrbitalProjectile orbital in projectiles)
                {
                    orbital.Tick();
                    if (orbital.LifeTime <= 0)
                    {
                        TryDoExplosion(orbital);
                        spentProjectiles.Add(orbital);
                    }
                }

            if (!spentProjectiles.NullOrEmpty())
                foreach (OrbitalProjectile orbital in spentProjectiles)
                    projectiles.Remove(orbital);
        }

        private void TryDoExplosion(OrbitalProjectile proj)
        {
            if (proj == null || Intercepted(proj.targetCell)) return;

            if (damage == null) // Used just in case all the other catches somehow fail
                damage = DamageDefOf.Bomb;

            if (extraGasType != 1)
            {
                GenExplosion.DoExplosion(proj.targetCell, Map, explosionRadiusRange.RandomInRange, damage, instigator, damageAmount, armorPenetration, explosionSound,
                    null, null, null, postExplosionThing, postExplosionThingChance, postExplosionSpawnThingCount, (GasType)extraGasType, false, preExplosionThing,
                    preExplosionThingChance, preExplosionSpawnThingCount, fireChance, damageFalloff, null, null, null, true, 1f, 0, true, postExplosionThingWater,
                    screenShakeFactor);
            }
            else
            {
                GenExplosion.DoExplosion(proj.targetCell, Map, explosionRadiusRange.RandomInRange, damage, instigator, damageAmount, armorPenetration,
                    explosionSound, null, null, null, postExplosionThing, postExplosionThingChance, postExplosionSpawnThingCount, null, false, preExplosionThing,
                    preExplosionThingChance, preExplosionSpawnThingCount, fireChance, damageFalloff, null, null, null, true, 1f, 0, true, postExplosionThingWater,
                    screenShakeFactor);
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (!projectiles.NullOrEmpty())
                foreach (OrbitalProjectile orbital in projectiles)
                    orbital.Draw(MaterialPool.MatFrom(projectileTexPath, ShaderDatabase.Transparent, projectileColor));
        }

        private bool Intercepted(IntVec3 targetCell)
        {
            if (!targetCell.IsValid || Map == null) return true;
            List<Thing> list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ProjectileInterceptor).Where((Thing t) => t.HasComp<CompProjectileInterceptor>()).ToList();

            if (!list.NullOrEmpty())
                foreach (Thing thing in list)
                {
                    CompProjectileInterceptor comp = thing.TryGetComp<CompProjectileInterceptor>();
                    if (comp != null && comp.Active && comp.Props.interceptAirProjectiles && targetCell.InHorDistOf(thing.Position, comp.Props.radius) &&
                        (comp.Props.interceptNonHostileProjectiles || instigator.HostileTo(thing)))
                        return true;
                }

            return false;
        }

        private void GetNextExplosionCell()
        {
            nextExplosionCell = (from x in GenRadial.RadialCellsAround(Position, impactAreaRadius, true)
                                 where x.InBounds(Map)
                                 select x).RandomElementByWeight((IntVec3 x) => DistanceChanceFactor.Evaluate(x.DistanceTo(Position) / impactAreaRadius));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref impactAreaRadius, "impactAreaRadius", 15f);
            Scribe_Values.Look(ref explosionsRemaining, "explosionsRemaining", 0);
            Scribe_Values.Look(ref explosionRadiusRange, "explosionRadiusRange", new FloatRange(6f, 8f));
            Scribe_Values.Look(ref randomFireRadius, "randomFireRadius", 25);
            Scribe_Values.Look(ref bombIntervalTicks, "bombIntervalTicks", 18);
            Scribe_Values.Look(ref warmupTicks, "warmupTicks", 0);
            Scribe_Values.Look(ref ticksToNextEffect, "ticksToNextEffect", 0);
            Scribe_Values.Look(ref projectileTexPath, "projectileTexPath", "Things/Projectile/Bullet_Big");
            Scribe_Values.Look(ref projectileColor, "projectileColor", Color.white);
            Scribe_Collections.Look(ref projectiles, "projectiles", LookMode.Deep);

            Scribe_Values.Look(ref damageAmount, "damageAmount", -1);
            Scribe_Values.Look(ref nextExplosionCell, "nextExplosionCell");
            Scribe_Values.Look(ref fireChance, "fireChance", 1f);
            Scribe_Values.Look(ref extraGasType, "extraGasType", 1);
            Scribe_Values.Look(ref damageDef, "damageDef", "Bomb");
            Scribe_Values.Look(ref armorPenetration, "armorPenetration", -1f);
            Scribe_Values.Look(ref explosionSoundDef, "explosionSoundDef", null);
            Scribe_Values.Look(ref postExplosionThingDef, "postExplosionThingDef", null);
            Scribe_Values.Look(ref postExplosionThingChance, "postExplosionThingChance", 0f);
            Scribe_Values.Look(ref postExplosionSpawnThingCount, "postExplosionSpawnThingCount", 1);
            Scribe_Values.Look(ref preExplosionThingDef, "preExplosionThingDef", null);
            Scribe_Values.Look(ref preExplosionThingChance, "preExplosionThingChance", 0f);
            Scribe_Values.Look(ref preExplosionSpawnThingCount, "preExplosionSpawnThingCount", 1);
            Scribe_Values.Look(ref damageFalloff, "damageFalloff", false);
            Scribe_Values.Look(ref postExplosionThingWaterDef, "postExplosionThingWaterDef", null);
            Scribe_Values.Look(ref screenShakeFactor, "screenShakeFactor", 0f);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (!nextExplosionCell.IsValid)
                    GetNextExplosionCell();
                if (projectiles == null)
                    projectiles = new List<OrbitalProjectile>();
            }

            if (DefDatabase<DamageDef>.GetNamedSilentFail(damageDef) != null)
                damage = DefDatabase<DamageDef>.GetNamed(damageDef);
            else
                damage = DamageDefOf.Bomb;

            if (explosionSoundDef != null && DefDatabase<SoundDef>.GetNamedSilentFail(explosionSoundDef) != null)
                explosionSound = DefDatabase<SoundDef>.GetNamed(explosionSoundDef);

            if (postExplosionThingDef != null && DefDatabase<ThingDef>.GetNamedSilentFail(postExplosionThingDef) != null)
                postExplosionThing = DefDatabase<ThingDef>.GetNamed(postExplosionThingDef);

            if (preExplosionThingDef != null && DefDatabase<ThingDef>.GetNamedSilentFail(preExplosionThingDef) != null)
                preExplosionThing = DefDatabase<ThingDef>.GetNamed(preExplosionThingDef);

            if (postExplosionThingWaterDef != null && DefDatabase<ThingDef>.GetNamedSilentFail(postExplosionThingWaterDef) != null)
                postExplosionThingWater = DefDatabase<ThingDef>.GetNamed(postExplosionThingWaterDef);
        }
    }
}
