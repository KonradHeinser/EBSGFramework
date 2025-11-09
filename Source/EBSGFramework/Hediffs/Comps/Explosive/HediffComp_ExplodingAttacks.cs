using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodingAttacks : HediffComp
    {
        public new HediffCompProperties_ExplodingAttacks Props => (HediffCompProperties_ExplodingAttacks)props;

        public bool currentlyExploding;
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
