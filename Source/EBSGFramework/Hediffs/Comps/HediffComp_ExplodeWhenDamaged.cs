using Verse;
namespace EBSGFramework
{
    public class HediffComp_ExplodeOnDamaged : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeWhenDamaged Props => (HediffCompProperties_ExplodeWhenDamaged)props;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            DoExplosion(parent.pawn.Position);
        }
    }
}
