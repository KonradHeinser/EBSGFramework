using Verse;
namespace EBSGFramework
{
    public class HediffComp_ExplosiveRetaliation : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplosiveRetaliation Props => (HediffCompProperties_ExplosiveRetaliation)props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity)
                if (dinfo.Instigator != null && (!(dinfo.Instigator is Pawn pawn) || !pawn.Dead))
                    DoExplosion(dinfo.Instigator.Position);
        }
    }
}
