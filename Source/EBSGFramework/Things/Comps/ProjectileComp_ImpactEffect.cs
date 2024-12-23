using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class ProjectileComp_ImpactEffect : ThingComp
    {
        private CompProperties_ProjectileImpactEffect Props => props as CompProperties_ProjectileImpactEffect;

        protected Projectile Projectile => parent as Projectile;

        public void Impact()
        {
            if (Projectile == null) return;

            if (Props.fleck != null)
                FleckMaker.Static(Projectile.DrawPos, parent.Map, Props.fleck, 1f);

            if (Props.mote != null)
                MoteMaker.MakeStaticMote(parent.DrawPos, parent.Map, Props.mote, 1f);

            if (Props.effecter != null)
            {
                Effecter effecter = Props.effecter.Spawn(parent.Position, parent.Map, 1f);
                effecter.offset = parent.DrawPos - parent.Position.ToVector3();
                effecter.Cleanup();
            }
        }

    }
}
