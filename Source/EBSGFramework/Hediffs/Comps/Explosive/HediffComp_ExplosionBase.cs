using Verse;

namespace EBSGFramework
{
    public abstract class HediffComp_ExplosionBase : HediffComp
    {
        // A base for all the explosive hediff comps which trigger more than once
        public bool CurrentlyExploding => explosionCooldown > 0;
        
        protected int explosionCooldown = -1;

        protected virtual int CooldownInterval => 10;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);
            if (CurrentlyExploding)
                explosionCooldown -= delta;
        }

        public virtual void StartCooldown()
        {
            explosionCooldown = CooldownInterval;
        }
    }
}