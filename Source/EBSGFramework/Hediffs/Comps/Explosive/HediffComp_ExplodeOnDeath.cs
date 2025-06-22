using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeOnDeath : HediffComp
    {
        public new HediffCompProperties_ExplodeOnDeath Props => (HediffCompProperties_ExplodeOnDeath)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            Props.explosion.DoExplosion(Pawn, Pawn.PositionHeld, Pawn.MapHeld, parent.Severity);
        }
    }
}
