using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplosiveRetaliation : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplosiveRetaliation Props => (HediffCompProperties_ExplosiveRetaliation)props;

        public int cooldownTicks = 0; // Not saved because this is just to avoid performance issues

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (cooldownTicks > 0) cooldownTicks--;
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (cooldownTicks <= 0 && parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity)
                if (dinfo.Instigator != null && (!(dinfo.Instigator is Pawn pawn) || !pawn.Dead))
                {
                    DoExplosion(dinfo.Instigator.Position);
                    cooldownTicks = Props.cooldownTicks;
                }
        }
    }
}
