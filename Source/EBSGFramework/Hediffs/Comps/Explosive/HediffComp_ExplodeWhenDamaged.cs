using Verse;
namespace EBSGFramework
{
    public class HediffComp_ExplodeOnDamaged : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeWhenDamaged Props => (HediffCompProperties_ExplodeWhenDamaged)props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (parent.Severity >= Props.minSeverity && parent.Severity < Props.maxSeverity)
                DoExplosion(parent.pawn.Position);
        }
    }
}
