using RimWorld;
using Verse;

namespace EBSGFramework
{
    public class CompProperties_ProjectileImpactEffect : CompProperties
    {
        public FleckDef fleck;
        public ThingDef mote;
        public EffecterDef effecter;

        public CompProperties_ProjectileImpactEffect()
        {
            this.compClass = typeof(ProjectileComp_ImpactEffect);
        }
    }
}
