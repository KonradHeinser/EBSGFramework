namespace EBSGFramework
{
    public class HediffComp_ExplodingRangedAttacks : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodingRangedAttacks Props => (HediffCompProperties_ExplodingRangedAttacks)props;

        public bool currentlyExploding = false;
        public int explosionCooldown = -1;

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (currentlyExploding && explosionCooldown < 0)
            {
                explosionCooldown = 10;
            }
            else if (currentlyExploding)
            {
                if (explosionCooldown == 0)
                {
                    currentlyExploding = false;
                }
                explosionCooldown--;
            }
        }
    }
}
