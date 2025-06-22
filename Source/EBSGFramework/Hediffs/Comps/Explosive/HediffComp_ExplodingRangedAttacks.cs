using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodingRangedAttacks : HediffComp
    {
        public new HediffCompProperties_ExplodingRangedAttacks Props => (HediffCompProperties_ExplodingRangedAttacks)props;

        public bool currentlyExploding = false;
        public int explosionCooldown = -1;

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (currentlyExploding && explosionCooldown < 0)
                explosionCooldown = 10;
            else if (currentlyExploding)
                if (explosionCooldown <= 0)
                    currentlyExploding = false;
                else
                    explosionCooldown -= delta;
        }
    }
}
