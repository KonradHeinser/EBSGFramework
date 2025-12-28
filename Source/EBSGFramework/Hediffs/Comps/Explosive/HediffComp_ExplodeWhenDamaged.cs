using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeWhenDamaged : HediffComp_ExplosionBase
    {
        public new HediffCompProperties_ExplodeWhenDamaged Props => (HediffCompProperties_ExplodeWhenDamaged)props;

        protected override int CooldownInterval => Props.cooldownTicks;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (!CurrentlyExploding && Props.validSeverities.ValidValue(parent.Severity))
            {
                Props.explosion.DoExplosion(Pawn, Pawn.PositionHeld, Pawn.MapHeld, parent.Severity);
                StartCooldown();
            }
        }
    }
}
