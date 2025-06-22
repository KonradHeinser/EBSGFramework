using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplosiveRetaliation : HediffComp
    {
        public new HediffCompProperties_ExplosiveRetaliation Props => (HediffCompProperties_ExplosiveRetaliation)props;

        public int cooldownTicks = 0; // Not saved because this is just to avoid performance issues


        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (cooldownTicks > 0)
                cooldownTicks -= delta;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && Props.validSeverities.ValidValue(parent.Severity))
                if (dinfo.Instigator != null && dinfo.Instigator is Pawn pawn && !pawn.Dead)
                {
                    Props.explosion.DoExplosion(Pawn, pawn.PositionHeld, pawn.MapHeld, parent.Severity);
                    cooldownTicks = Props.cooldownTicks;
                }
        }
    }
}
