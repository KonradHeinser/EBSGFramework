using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplosiveRetaliation : HediffComp_ExplosionBase
    {
        public new HediffCompProperties_ExplosiveRetaliation Props => (HediffCompProperties_ExplosiveRetaliation)props;
        
        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (explosionCooldown <= 0 && Props.validSeverities.ValidValue(parent.Severity))
                if (dinfo.Instigator is Pawn pawn && !pawn.Dead)
                {
                    Props.explosion.DoExplosion(Pawn, pawn.PositionHeld, pawn.MapHeld, parent.Severity);
                    explosionCooldown = Props.cooldownTicks;
                }
        }
    }
}
