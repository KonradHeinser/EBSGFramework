using System;
namespace EBSGFramework
{
    public class HediffComp_ExplodingMeleeAttacks : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodingMeleeAttacks Props => (HediffCompProperties_ExplodingMeleeAttacks)props;

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
