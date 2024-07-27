using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeWhenDamaged : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeWhenDamaged Props => (HediffCompProperties_ExplodeWhenDamaged)props;

        public int cooldownTicks = 0; // Not saved because this is just to avoid performance issues

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (cooldownTicks > 0) cooldownTicks--;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity)
            {
                DoExplosion(parent.pawn.Position);
                cooldownTicks = Props.cooldownTicks;
            }
        }
    }
}
