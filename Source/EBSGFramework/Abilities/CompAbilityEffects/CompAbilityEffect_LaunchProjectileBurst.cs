using RimWorld;
using Verse;
using Verse.Sound;

namespace EBSGFramework
{
    public class CompAbilityEffect_LaunchProjectileBurst : CompAbilityEffect
    {
        public new CompProperties_AbilityLaunchProjectileBurst Props => (CompProperties_AbilityLaunchProjectileBurst)props;

        public int ticksToShot = -1;

        public int shotsLeft;

        public LocalTargetInfo curTarget;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            shotsLeft = Props.burstCount;
            curTarget = target;
            if (Props.immediateLaunch)
                LaunchProjectile(target);
            ticksToShot = Props.ticksBetweenShots;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToShot, "ticksToShot");
            Scribe_Values.Look(ref shotsLeft, "shotsLeft");
            Scribe_TargetInfo.Look(ref curTarget, false, "curTarget");
        }

        public override void CompTick()
        {
            base.CompTick();
            
            if (shotsLeft == 0)
                return;

            if (curTarget == null || !curTarget.IsValid || (curTarget.HasThing && (!curTarget.Thing.Spawned || curTarget.Thing.Destroyed)))
            {
                shotsLeft = 0;
                curTarget = null;
                return;
            }

            if (ticksToShot > 0)
            {
                ticksToShot--;
                return;
            }

            ticksToShot += Props.ticksBetweenShots;
            LaunchProjectile(curTarget);
        }

        /*
        public override void CompTickInterval(int delta)
        {
            base.CompTickInterval(delta);

            if (shotsLeft == 0)
                return;

            if (curTarget == null || !curTarget.IsValid || (curTarget.HasThing && (!curTarget.Thing.Spawned || curTarget.Thing.Destroyed)))
            {
                shotsLeft = 0;
                curTarget = null;
                return;
            }

            if (ticksToShot > 0)
            {
                ticksToShot -= delta;
                return;
            }

            ticksToShot += Props.ticksBetweenShots;
            LaunchProjectile(curTarget);
        }
*/
        public virtual void LaunchProjectile(LocalTargetInfo target)
        {
            shotsLeft--;
            Projectile proj = GenSpawn.Spawn(Props.projectileDef, parent.pawn.Position, parent.pawn.Map) as Projectile;
            proj.Launch(parent.pawn, parent.pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);

            Props?.firingSound?.PlayOneShot(new TargetInfo(parent.pawn.Position, parent.pawn.Map));

            if (shotsLeft == 0)
                curTarget = null;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Thing != null;
        }
    }
}
