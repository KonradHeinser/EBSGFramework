using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeWhenDamaged : HediffComp
    {
        public new HediffCompProperties_ExplodeWhenDamaged Props => (HediffCompProperties_ExplodeWhenDamaged)props;

        public int cooldownTicks; // Not saved because this is just to avoid performance issues

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (cooldownTicks > 0)
                cooldownTicks -= delta;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && Props.validSeverities.ValidValue(parent.Severity))
            {
                Props.explosion.DoExplosion(Pawn, Pawn.PositionHeld, Pawn.MapHeld, parent.Severity);
                cooldownTicks = Props.cooldownTicks;
            }
        }
    }
}
