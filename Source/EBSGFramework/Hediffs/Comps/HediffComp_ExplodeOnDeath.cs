using Verse;

namespace EBSGFramework
{
    public class HediffComp_ExplodeOnDeath : BurstHediffCompBase
    {
        public new HediffCompProperties_ExplodeOnDeath Props => (HediffCompProperties_ExplodeOnDeath)props;

        public override void Notify_PawnDied()
        {
            Log.Message("Pawn died");
            DoExplosion(parent.pawn.Corpse.Position);
        }
    }
}
